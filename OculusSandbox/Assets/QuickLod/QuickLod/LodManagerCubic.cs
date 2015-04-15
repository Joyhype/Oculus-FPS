// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LodManagerCubic.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright> 
// <summary>
//   A lod manager that uses a grid for all updates
//   It creates a grid and manages only cells that have at least one lod object inside
//   All cells outside the range of any lod source are being ignored by the lod calculations
//   Each cell can have a priority from zero to four, the nearer the cell is to a lod source, the higher the priority
//   When a lod object is registered, it is asigned to a cell and will only be updated when the cell has a priority
//   The higher the priority of a cell, the more objects of it will be updated per frame (depending on updates per frame)
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using QuickLod.Attributes;
    using QuickLod.Containers;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    using UnityEngine;

    /// <summary>
    /// A lod manager that uses a grid for all updates<br/><br/>
    /// It creates a grid and manages only cells that have at least one lod object inside<br />
    /// All cells outside the range of any lod source are being ignored by the lod calculations<br />
    /// Each cell can have a priority from zero to four, the nearer the cell is to a lod source, the higher the priority<br/><br/>
    /// When a lod object is registered, it is assigned to a cell and will only be updated when the cell has a priority<br/>
    /// The higher the priority of a cell, the more objects of it will be updated per frame (depending on updates per frame)
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Quick Lod/Lod Manager Cubic")]
    [LodHelper(@"This component manages all lod objects in the scene.
You must add this component to the scene for QuickLod to work.

It uses a grid to optimize the managing process.
You need to define the grid with in the ""Grid setup"" field.
The grid should cover all the space where a lod object can be.

With the ""Cell update interval"", you can define how often the cell priority is updated.
To include objects outside the grid, you can activate the ""Clamp objects to grid"" option.
The ""Recalculate"" functions optimize the grid for the current scene setup.

The update speed can be set in the ""Updates per frame"" field.
The larger the value, the faster will QuickLod update the lod objects, but it will need more cpu power.

In the editor settings, you can define how QuickLod should work in the Unity editor.
Those settings have no effect when the game is running!
Don't forget to set a good value for the ""Viewport update distance"" (see LodSource help)")]
    public class LodManagerCubic : LodManagerBase
    {
        #region constants

        /// <summary>
        /// The minimal allowed cell size for the grid
        /// </summary>
        /// <remarks>
        /// You can change this value in code.<br/>
        /// It's not recommended to use smaller cell sizes than 5 as it somehow defeats the purpose of a grid.
        /// </remarks>
        /// <value>
        /// Default: 5
        /// </value>
        public const float MinCellSize = 5;

        /// <summary>
        /// The maximum amount of cells for each axis
        /// </summary>
        /// <remarks>
        /// This bottleneck prevents memory overflow.<br/>
        /// You can increase this number if you want more cells per axis.
        /// </remarks>
        /// <value>
        /// Default: 500
        /// </value>
        public const float MaxCellsPerAxis = 500;

        /// <summary>
        /// The minimum grid size
        /// </summary>
        /// <remarks>
        /// Don't go bellow 0
        /// </remarks>
        /// <value>Default: 1</value>
        public const float MinGridSize = 1;

        /// <summary>
        /// The grid margin
        /// </summary>
        public const float GridMargin = 5;

        /// <summary>
        /// The target amount of cells
        /// </summary> 
        public const float TargetAmountOfCells = 2000;

        #endregion

        #region private fields

        /// <summary>
        /// Contains all active cells of the first priority
        /// </summary>
        private HashSet<GridCell>[] cellPrioritySets;

        /// <summary>
        /// The calculations budget for each cell priority level
        /// </summary>
        private int[] cellPriorityUpFs;

        /// <summary>
        /// Stores all objects which want to be deregistered
        /// </summary>
        private List<LodObjectBase> objectsToRegister;

        /// <summary>
        /// Stores all objects which want to be registered
        /// </summary>
        private List<LodObjectBase> objectsToDeregister;

        /// <summary>
        /// Contains all cells which need to deactivate their content
        /// </summary>
        private List<GridCell> cellsToDeactivate;

        /// <summary>
        /// The amount of cells which can maximally be stored in the current grid configuration.
        /// </summary>
        [SerializeField]
        private int amountOfCells;

        /// <summary>
        /// The amount of cells which are currently used
        /// </summary>
        [SerializeField]
        private int amountOfUsedCells;

        /// <summary>
        /// Backing field for <see cref="CellUpdateInterval"/>
        /// </summary>
        [SerializeField]
        private int cellUpdateInterval;

        /// <summary>
        /// Backing field for <see cref="MaxCellDeactivations"/>
        /// </summary>
        [SerializeField]
        private int maxCellDeactivations;

        /// <summary>
        /// Backing field for <see cref="GridSize"/>
        /// </summary>
        [SerializeField]
        private Vector3 gridSize;

        /// <summary>
        /// Backing field for <see cref="GridStart"/>
        /// </summary>
        [SerializeField]
        private Vector3 gridStart;

        /// <summary>
        /// The grid data used to store and manage lod objects 
        /// </summary>
        private Dictionary<int, GridColumn> gridData;

        /// <summary>
        /// The amount of columns
        /// </summary>
        private int columnAmount;

        /// <summary>
        /// The amount of rows per column
        /// </summary>
        private int rowAmount;

        /// <summary>
        /// The amount of cells per row
        /// </summary>
        private int cellAmount;

        /// <summary>
        /// Backing field for <see cref="CellSize"/>
        /// </summary>
        [SerializeField]
        private Vector3 cellSize;

        /// <summary>
        /// Obsolete: Only used for porting projects prior to QuickLod version 1.5 to version 1.5 or later
        /// </summary>
        [SerializeField]
        private float chunkSize;

        /// <summary>
        /// Backing field for <see cref="ClampObjectsToGrid"/>
        /// </summary>
        [SerializeField]
        private bool clampObjectsToGrid;
        
        /// <summary>
        /// The counter is used for the cell update delay
        /// </summary>
        private int counter;

#if UNITY_EDITOR

        /// <summary>
        /// Stores the previous value of <see cref="GridSize"/><br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private Vector3 prevGSi;

        /// <summary>
        /// Stores the previous value of <see cref="GridStart"/><br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private Vector3 prevGSt;

        /// <summary>
        /// Stores the previous value of <see cref="CellSize"/><br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private Vector3 prevCS;
        
        /// <summary>
        /// Stores the previous value of <see cref="ClampObjectsToGrid"/><br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private bool prevCOTG;

        /// <summary>
        /// The draw cell gizmos.<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        private bool drawCellGizmos;

        /// <summary>
        /// The draw cell level gizmos.<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        private bool drawCellLevelGizmos;

        /// <summary>
        /// The draw grid gizmos.<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        private bool drawGridGizmos;

        /// <summary>
        /// The draw when deselected.<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        private bool drawWhenDeselected;

        /// <summary>
        /// The draw object cell gizmos.<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        private bool drawObjectCellGizmos;

        /// <summary>
        /// The draw managed objects.<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        private bool drawManagedObjects;

        /// <summary>
        /// The draw disabled objects.<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        private bool drawDisabledObjects;

        /// <summary>
        /// The draw in debug<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        private bool drawInDebug;

        /// <summary>
        /// The managed objects material.<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        private Material managedObjectsMaterial;

        /// <summary>
        /// Stores all cells that were drawn (or have to be drawn) for the object cell gizmos<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [NonSerialized]
        private HashSet<GridCell> usedCells;
#endif

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LodManagerCubic"/> class.
        /// </summary>
        public LodManagerCubic()
        {
            this.gridSize = new Vector3(100, 100, 100); 
            this.gridStart = new Vector3(-50, -50, -50);
            this.cellSize = new Vector3(20, 20, 20);
            this.chunkSize = -1;
            this.cellUpdateInterval = 5;
            this.maxCellDeactivations = 500;
            this.gridData = null;
            this.clampObjectsToGrid = true;
            
            this.cellPrioritySets = new HashSet<GridCell>[PriorityLevelAmount];
            this.cellPriorityUpFs = new int[PriorityLevelAmount];
            for (var i = 0; i < PriorityLevelAmount; i++)
            {
                this.cellPrioritySets[i] = new HashSet<GridCell>();
            }

            this.amountOfCells = 0;
            this.amountOfUsedCells = 0;

            this.objectsToRegister = new List<LodObjectBase>();
            this.objectsToDeregister = new List<LodObjectBase>();
            this.cellsToDeactivate = new List<GridCell>();
            this.gridData = new Dictionary<int, GridColumn>(0);
            
#if UNITY_EDITOR
            this.prevGSi = this.gridSize;
            this.prevGSt = this.gridStart;
            this.prevCS = this.cellSize;

            this.drawCellGizmos = false;
            this.drawCellLevelGizmos = false;
            this.drawGridGizmos = true; 
            this.drawObjectCellGizmos = false;
            this.drawWhenDeselected = false;
            this.drawManagedObjects = false;
            this.drawDisabledObjects = false;
            this.managedObjectsMaterial = null;
            this.usedCells = new HashSet<GridCell>();
#endif
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the amount of cells which can maximally be stored in the current grid configuration.
        /// </summary>
        public int AmountOfCells
        {
            get
            {
                return this.amountOfCells;
            }

            private set
            {
                this.amountOfCells = value;
            }
        }

        /// <summary>
        /// Gets the amount of cells which are currently used.
        /// </summary> 
        public int AmountOfUsedCells
        {
            get
            {
                return this.amountOfUsedCells;
            }
            private set
            {
                this.amountOfUsedCells = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval in which each cell level gets updated
        /// </summary>
        /// <remarks>
        /// When you have a loot of cells, this property can have a huge impact.<br/>
        /// Try to set this value as high as possible, so that your main character cannot walk faster than the cells update.<br/>
        /// Per cell update, your main character should not be able to walk through a whole cell.
        /// </remarks>
        public int CellUpdateInterval
        {
            get
            {
                return this.cellUpdateInterval;
            }

            set
            {
                this.cellUpdateInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of cells which may be deactivated per frame
        /// </summary>
        /// <value>
        /// The maximum cell deactivations.
        /// </value>
        public int MaxCellDeactivations
        {
            get
            {
                return this.maxCellDeactivations;
            }

            set
            {
                this.maxCellDeactivations = Mathf.Max(value, 1);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the objects outside of the grid should be registered to the nearest cell
        /// </summary>
        /// <remarks>
        /// It is recommended to define your grid size so, that you don't need this function.<br/>
        /// This function is meant for if you have objects that sometimes leave the grid area.
        /// </remarks>
        public bool ClampObjectsToGrid
        {
            get
            {
                return this.clampObjectsToGrid;
            }

            set
            {
                if (this.clampObjectsToGrid == value)
                {
                    return;
                }

                this.clampObjectsToGrid = value;

#if UNITY_EDITOR
                this.prevCOTG = this.clampObjectsToGrid;
#endif

                this.UpdateGrid();
            }
        }

        /// <summary>
        /// Gets or sets the size of the grid
        /// </summary>
        /// <remarks>
        /// Changing this value can cause a lot of recalculations.<br/>
        /// It is recommended to leave a short pause between changes of this value.
        /// </remarks>
        public Vector3 GridSize
        {
            get
            {
                return this.gridSize;
            }

            set
            {
                if (this.gridSize == value)
                {
                    return;
                }

                this.gridSize = value;

                if (this.gridSize.x < MinGridSize)
                {
                    this.gridSize.x = MinGridSize;
                }

                if (this.gridSize.y < MinGridSize)
                {
                    this.gridSize.y = MinGridSize;
                }

                if (this.gridSize.z < MinGridSize)
                {
                    this.gridSize.z = MinGridSize;
                }

                this.CheckCellSize();

#if UNITY_EDITOR
                this.prevGSi = this.gridSize;
#endif
                this.UpdateGrid();
            }
        }

        /// <summary>
        /// Gets or sets the start position of the grid
        /// </summary>
        /// <remarks>
        /// Changing this value can cause a lot of recalculations.<br/>
        /// It is recommended to leave a short pause between changes of this value.
        /// </remarks>
        public Vector3 GridStart
        {
            get
            {
                return this.gridStart;
            }

            set
            {
                if (this.gridStart == value)
                {
                    return;
                }

                this.gridStart = value;
#if UNITY_EDITOR
                this.prevGSt = this.gridStart;
#endif

                this.UpdateGrid();
            }
        }

        /// <summary>
        /// Gets or sets the chunk size
        /// OBSOLETE: Use the property <see cref="CellSize"/> instead 
        /// </summary>
        /// <value>
        /// The size of the chunk.
        /// </value>
        [Obsolete("Use the property CellSize instead.")]
        public float ChunkSize
        {
            get
            {
                return this.cellSize.magnitude;
            }

            set
            {
                this.cellSize = new Vector3(value, value, value);
            }
        }

        /// <summary>
        /// Gets or sets the size of the cells
        /// </summary>
        /// <remarks> 
        /// Changing this value can cause a lot of recalculations.<br/>
        /// It is recommended to leave a short pause between changes of this value.
        /// </remarks>
        public Vector3 CellSize
        {
            get
            {
                return this.cellSize;
            }

            set
            {
                if (this.cellSize == value)
                {
                    return;
                }

                this.cellSize = value;

                this.CheckCellSize();

#if UNITY_EDITOR
                this.prevCS = this.cellSize;
#endif

                this.UpdateGrid();
            }
        }

        /// <summary>
        /// Gets the optimal border width for source update distances
        /// </summary>
        /// <value>
        /// The optimal source border.
        /// </value>
        public override float OptimalSourceBorder
        {
            get
            {
                return this.cellSize.magnitude / 2;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Registers a new lod object
        /// </summary>
        /// <param name="lodObject">
        /// The lod object that should be registered
        /// </param>
        public override void RegisterObject(LodObjectBase lodObject)
        {
            // Add the object in the next update cycle
            this.objectsToRegister.Add(lodObject);
        }

        /// <summary>
        /// Deregisters an existing lod object
        /// </summary>
        /// <param name="lodObject">
        /// The lod object that should be deregistered
        /// </param>
        public override void DeregisterObject(LodObjectBase lodObject)
        {
            // Remove the object in the next update cycle
            this.objectsToDeregister.Add(lodObject);
        }

        /// <summary>
        /// Handles source movement
        /// </summary>
        /// <param name="source">The source.</param>
        public override void HandleSourceMovement(LodSource source, Vector3 previousPosition)
        {
            // When the flag is already set, the movement doesn't need to be handled
            if (this.HasSourceUpdates)
            {
                return;
            }

            var newCellIndex = this.WorldToCellPosition(source.Transform.position);
            var oldCellIndex = this.WorldToCellPosition(previousPosition);

            // When the source changed the cell, update
            if (newCellIndex != oldCellIndex)
            {
                this.HasSourceUpdates = true;
            }
        }

        /// <summary>
        /// Recalculates in which cell the object is
        /// </summary>
        /// <param name="lodObject">
        /// The lod object that needs recalculation
        /// </param>
        public override void UpdateObjectPosition(LodObjectBase lodObject)
        {
            if (lodObject == null)
            {
                return;
            }

            // Caches the position value
            var pos = this.WorldToCellPosition(lodObject.Transform.position);
            
            if (this.clampObjectsToGrid)
            {
                pos.X = Math.Max(Math.Min(pos.X, this.columnAmount - 1), 0);
                pos.Y = Math.Max(Math.Min(pos.Y, this.rowAmount - 1), 0);
                pos.Z = Math.Max(Math.Min(pos.Z, this.cellAmount - 1), 0);
            }
            else
            {
                // If the object is outside the bounds, remove it from the grid
                if ((pos.X < 0 || pos.X > this.columnAmount - 1) || (pos.Y < 0 || pos.Y > this.rowAmount - 1)
                    || (pos.Z < 0 || pos.Z > this.cellAmount - 1))
                {
                    this.RemoveObjectFromCell(lodObject);
                    return;
                }
            }

            var cell = this.GetCellAtIndex(pos.X, pos.Y, pos.Z, true);

            // When the previous cell is not equal the new cell, remove the object from the old cell
            if (lodObject.ParentCell != cell)
            {
                this.RemoveObjectFromCell(lodObject);
                cell.Content.Add(lodObject);
                lodObject.ParentCell = cell;
            }

            // If the parent cell is hidden, hide the object too
            if (lodObject.ParentCell.SourcesInRange.Count == 0)
            {
                this.RegisterCellForHiding(cell);
            }
        }

        /// <summary>
        /// Calculates the world position of the given cell position
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>Returns the center position of the cell</returns>
        public Vector3 CellToWorldPosition(IntVector3 position)
        {
            var semiCellSize = this.cellSize / 2f;
            return IntVector3.Scale(position, this.cellSize) + semiCellSize;
        }

        /// <summary>
        /// Calculates the cell position from the given world position
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>Returns the cell index position</returns>
        public IntVector3 WorldToCellPosition(Vector3 position)
        {
            return new IntVector3(
                Mathf.FloorToInt((position.x - this.gridStart.x) / this.cellSize.x),
                Mathf.FloorToInt((position.y - this.gridStart.y) / this.cellSize.y),
                Mathf.FloorToInt((position.z - this.gridStart.z) / this.cellSize.z));
        }

        /// <summary>
        /// Calculates the grid space based on the objects in the scene and the object this component is on
        /// </summary>
        /// <remarks>
        /// The grid size is calculated like this:
        /// 1) A bounding box around all lod objects in the scene is calculated
        /// 2) A bounding box around the game object of this component is calculated (with the mesh or terrain data)
        /// 3) The two bounding boxes are combined
        /// 4) A margin is added to the resulting bounding box (<see cref="GridMargin"/>)
        /// 5) The grid size is applied and the cell size get's recalculated
        /// </remarks> 
        public void RecalculateGridSpace()
        {
            var bounds = new Bounds();

            var lodObjs = FindObjectsOfType(typeof(LodObjectBase)) as LodObjectBase[];
            if (lodObjs == null)
            {
                return;
            }

            foreach (var lodObj in lodObjs)
            {
                bounds.Encapsulate(lodObj.transform.position);
            }

            var meshRenderer = this.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
            var terrainScript = this.gameObject.GetComponent(typeof(Terrain)) as Terrain;

            if (terrainScript != null)
            {
                bounds.Encapsulate(this.Transform.position);
                bounds.Encapsulate(this.Transform.position + terrainScript.terrainData.size);
            }
            else if (meshRenderer != null)
            {
                bounds.Encapsulate(meshRenderer.bounds);
            }

            var gridMargin = Vector3.one * GridMargin;
            this.gridStart = bounds.min - gridMargin;
            this.gridSize = bounds.size + (gridMargin * 2);

            // Rounds the values
            this.gridStart = new Vector3(
                Mathf.Floor(this.gridStart.x), Mathf.Floor(this.gridStart.y), Mathf.Floor(this.gridStart.z));
            this.gridSize = new Vector3(
                Mathf.Ceil(this.gridSize.x), Mathf.Ceil(this.gridSize.y), Mathf.Ceil(this.gridSize.z));

            this.RecalculateCellSize();
        }

        /// <summary>
        /// Calculates the cell size
        /// </summary> 
        /// <remarks>
        /// This method calculates a cell size to get a cell amount equal to <see cref="TargetAmountOfCells"/>
        /// </remarks>
        public void RecalculateCellSize()
        {
            this.cellSize = Vector3.one
                            * Mathf.CeilToInt(
                                Mathf.Pow(
                                    ((this.gridSize.x + 1) * (this.gridSize.y + 1) * (this.gridSize.z + 1))
                                    / TargetAmountOfCells,
                                    0.33333f));

            this.CheckCellSize(); 
            this.UpdateGrid();
        }

#if UNITY_EDITOR

        /// <summary>
        /// Draws the gizmos for a lod object<br/>
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        /// <param name="lodObjectBase">
        /// The lod object that wants to draw his gizmos
        /// </param>
        public override void DrawLodObjectGizmos(LodObjectBase lodObjectBase)
        {
            // When the gizmos is deactivated, the object is deactivated or the object is not registered, don't draw the gizmos
            if (!this.enabled || !this.drawObjectCellGizmos || lodObjectBase.enabled == false ||lodObjectBase.ParentCell == null)
            {
                return;
            }

            // Local variables for better code readability
            var cell = lodObjectBase.ParentCell;

            // Don't draw cells that are already drawn
            if (this.usedCells.Contains(cell))
            {
                return;
            }

            // Remember the drawn cell
            this.usedCells.Add(cell);

            // The selected cell needs to be drawn over all other gizmos, when other gizmos is drawn, then don't draw the object cell here
            if (!this.drawWhenDeselected)
            {
                var halfCellSize = this.cellSize / 2f;
                var center = new Vector3(
                    this.gridStart.x + (cell.Parent.Parent.Index * this.cellSize.x) + halfCellSize.x,
                    this.gridStart.y + (cell.Parent.Index * this.cellSize.y) + halfCellSize.y, 
                    this.gridStart.z + (cell.Index * this.cellSize.z) + halfCellSize.z);

                Gizmos.color = new Color(0, 0, 1);
                Gizmos.DrawWireCube(center, this.cellSize);
            }
        }

#endif

        #endregion

        #region protected methods

        /// <summary>
        /// Can be used as "Update()" method for both the editor and the game
        /// </summary>
        protected override void NextStep()
        {
            base.NextStep();

            // To prevent changes to the grid during the update cycle, apply all changes before
            if (this.objectsToDeregister.Count > 0)
            {
                foreach (var lodObject in this.objectsToDeregister)
                {
                    this.RemoveObjectFromCell(lodObject);
                }

                this.objectsToDeregister = new List<LodObjectBase>();
            }

            if (this.objectsToRegister.Count > 0)
            {
                foreach (var lodObject in this.objectsToRegister)
                {
                    // Adds the lod object to the correct cell
                    this.UpdateObjectPosition(lodObject);
                }

                this.objectsToRegister = new List<LodObjectBase>();
            }

            // Deactivate the content of the next bunch of cells
            if (this.cellsToDeactivate.Count > 0)
            {
                var index = this.cellsToDeactivate.Count - 1;
                for (var i = 0; i < this.MaxCellDeactivations && i < this.cellsToDeactivate.Count; i++)
                {
                    var cell = this.cellsToDeactivate[index];
                    cell.HideContent();
                    cell.IsMarkedForHiding = false;

                    this.cellsToDeactivate.RemoveAt(index);
                    index--;
                }
            }

            // Now launch the actual update cycle
            this.CalculateNextObjects();
        }

        /// <summary>
        /// Is called once per frame<br />
        /// This method is not reliable in the editor.<br />
        /// Use <see cref="NextStep" /> for a reliable update notification instead.
        /// </summary>
        protected override void Update()
        {
            base.Update();

#if UNITY_EDITOR
            if (this.drawManagedObjects && this.managedObjectsMaterial != null && (Selection.Contains(this.gameObject) || this.drawWhenDeselected) && (!Application.isPlaying || this.drawInDebug))
            {
                foreach (var lodObject in from column in this.gridData
                                          from row in column.Value.Rows
                                          from cell in row.Value.Cells
                                          from lodObject in cell.Value.Content
                                          where lodObject != null && !lodObject.ExcludeFromManager
                                          select lodObject)
                {
                    this.DrawChildObjects(lodObject.gameObject);
                }
            }
#endif
        }

        /// <summary>
        /// Is called when the object gets enabled
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            // When upgrading from QuickLod version 1.4 or older to a newer version, port the cell size value to the cell size value
            if (this.chunkSize >= 5) 
            {
                this.cellSize = new Vector3(this.chunkSize, this.chunkSize, this.chunkSize);
                this.chunkSize = -1;
            }

            // Reinitializes everything
            this.gridData = new Dictionary<int, GridColumn>();
            this.cellPrioritySets = new HashSet<GridCell>[PriorityLevelAmount];
            this.cellPriorityUpFs = new int[PriorityLevelAmount];
            for (var i = 0; i < PriorityLevelAmount; i++)
            {
                this.cellPrioritySets[i] = new HashSet<GridCell>();
            }

            this.UpdateGrid();

            // Registers all available and enabled lod objects
            var allLodObjects = (LodObjectBase[])FindObjectsOfType(typeof(LodObjectBase));
            foreach (var obj in allLodObjects)
            {
                obj.Register();
                obj.AddToPositionUpdateCallback();
            }
            
#if UNITY_EDITOR
            this.usedCells = new HashSet<GridCell>();
#endif
        }

        /// <summary>
        /// Gets called when the object was disabled<br />
        /// DON'T USE: This method must only be called by the Unity engine
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            // Deregisters all registered lod objects
            foreach (var lodObject in from column in this.gridData
                                      from row in column.Value.Rows
                                      from cell in row.Value.Cells
                                      from lodObject in cell.Value.Content
                                      where lodObject != null && !lodObject.ExcludeFromManager
                                      select lodObject)
            {
                lodObject.ParentCell = null;
            }

            // Clean up
            this.gridData = null;
            this.cellPrioritySets = null;
        }

        protected override void Reset()
        {
            base.Reset();

            // Calculate the optimal grid space and update the grid
            this.RecalculateGridSpace();
            
#if UNITY_EDITOR
            this.ViewportMaxUpdateDistance = LodSource.CalculateMaxDistance();
#endif
        }

        /// <summary>
        /// Called when the amount of allowed updates per frame has changed
        /// </summary>
        protected override void OnUpdatesPerFrameChanged()
        {
            base.OnUpdatesPerFrameChanged();
            this.UpdateCellUpF();
        }

        #endregion

        #region private methods

        /// <summary>
        /// Removes an object from the cell
        /// </summary>
        /// <param name="lodObject">
        /// The object that should be removed
        /// </param>
        private void RemoveObjectFromCell(LodObjectBase lodObject)
        {
            // Ignore objects that are not registered to any cell
            if (lodObject.ParentCell == null)
            {
                return;
            }

            var cell = lodObject.ParentCell;
            cell.Content.Remove(lodObject);
            lodObject.ParentCell = null;

            // Removes empty cells
            if (cell.Content.Count == 0)
            {
                this.HasSourceUpdates = true;
                var row = cell.Parent;
                this.amountOfUsedCells--;
                
                // Also remove empty rows and columns
                if (row.Cells.Count == 1)
                {
                    var column = row.Parent;
                    if (column.Rows.Count == 1)
                    {
                        column.Parent.Remove(column.Index);
                        return;
                    }

                    column.Rows.Remove(row.Index);
                    return;
                }

                row.Cells.Remove(cell.Index);
            }
        }

        /// <summary>
        /// Gets the index of the cell at.
        /// </summary>
        /// <param name="x">The x index.</param>
        /// <param name="y">The y index.</param>
        /// <param name="z">The z index.</param>
        /// <param name="createIfNotExistant">if set to <c>true</c> create all cell references that don't exist at the given indexes.</param>
        /// <returns>
        /// Returns the found or created cell.<br/>
        /// Returns null if <see cref="createIfNotExistant"/> is set to false and the cell doesn't exist
        /// </returns>
        private GridCell GetCellAtIndex(int x, int y, int z, bool createIfNotExistant)
        {
            GridColumn column;
            if (!this.gridData.TryGetValue(x, out column))
            {
                if (createIfNotExistant)
                {
                    column = new GridColumn(this.rowAmount, x, this.gridData);
                    this.gridData.Add(x, column);
                }
                else
                {
                    return null;
                }
            }

            GridRow row;
            if (!column.Rows.TryGetValue(y, out row))
            {
                if (createIfNotExistant)
                {
                    row = new GridRow(this.cellAmount, y, column);
                    column.Rows.Add(y, row);
                }
                else
                {
                    return null;
                }
            }

            GridCell cell;
            if (!row.Cells.TryGetValue(z, out cell))
            {
                if (createIfNotExistant)
                {
                    cell = new GridCell(z, row);
                    row.Cells.Add(z, cell);
                    this.amountOfUsedCells++;
                    this.HasSourceUpdates = true;
                }
                else
                {
                    return null;
                }
            }

            return cell;
        }
        
        /// <summary>
        /// Calculates the next objects, depending on the updates per frame
        /// </summary>
        private void CalculateNextObjects()
        {
            // Check for cell updates with a delay defined by cell update interval
            if (this.counter < this.CellUpdateInterval)
            {
                this.counter++;
            }
            else if (this.HasSourceUpdates)
            {
                this.UpdateCellLevels();
                
                this.counter = 0;
                this.HasSourceUpdates = false;
            }

            // Update the lodObjects in the cells
            for (var i = 0; i < PriorityLevelAmount; i++)
            {
                foreach (var cell in this.cellPrioritySets[i])
                {
                    this.CalculateCellItems(cell, this.cellPriorityUpFs[i]);
                }
            }
        }

        /// <summary>
        /// Calculates the next items for one cell
        /// </summary>
        /// <param name="cell">
        /// The cell of which the objects should be calculated
        /// </param>
        /// <param name="maxAmount">
        /// The maximum amount of objects to update
        /// </param>
        private void CalculateCellItems(GridCell cell, int maxAmount)
        {
            // jump over cells which are empty
            if (cell == null || cell.Content == null || cell.Content.Count <= 0)
            {
                return;
            }

            var sources = cell.SourcesInRange;

            // Only calculate as many objects as allowed (defined by priority and updates per frame)
            for (var i = 1; i <= maxAmount; i++)
            {
                // Get the next index
                var index = (cell.PreviousIndex + i) % cell.Content.Count;

                UpdateLodObjectDistance(cell.Content[index], sources);
                
                // When all lod objects of this cell have been calculated in this frame, go to the next cell
                if (index == cell.PreviousIndex)
                {
                    return;
                }

                // Remember the actual index
                if (i == maxAmount)
                {
                    cell.PreviousIndex = index;
                }
            }
        }

        /// <summary>
        /// Updates the priority of the cells
        /// </summary>
        private void UpdateCellLevels()
        {
            // Caches some calculation values to prevent redundant recalculations
            var halfCellSize = this.cellSize / 2f;
            var relativeGridStart = new Vector3(
                this.gridStart.x + halfCellSize.x, this.gridStart.y + halfCellSize.y, this.gridStart.z + halfCellSize.z);

            var sourceBounds = new IntBounds3[this.Sources.Count];

            // Reset the old leveled cells
            foreach (var cellPrioritySet in this.cellPrioritySets)
            {
                foreach (var cell in cellPrioritySet)
                {
                    cell.SourcesInRange = new List<LodSource>();
                }
            }

            // Generate the boundaries for all lod sources
            for (var i = 0; i < this.Sources.Count; i++)
            {
                var source = this.Sources[i];
                var size = Vector3.one * ((source.MaxUpdateDistance * source.DistanceMultiplier) * 2);
                var start = source.Transform.position - this.GridStart - (size / 2);

                sourceBounds[i] = new IntBounds3
                                      {
                                          X = Mathf.FloorToInt(start.x / this.cellSize.x),
                                          Y = Mathf.FloorToInt(start.y / this.cellSize.y),
                                          Z = Mathf.FloorToInt(start.z / this.cellSize.z),
                                          SizeX = Mathf.CeilToInt(size.x / this.cellSize.x) + 1,
                                          SizeY = Mathf.CeilToInt(size.y / this.cellSize.y) + 1,
                                          SizeZ = Mathf.CeilToInt(size.z / this.cellSize.z) + 1
                                      };
            }

            var cellsWithPriorityLevel = new HashSet<GridCell>();

            // For each source find the new cell levels
            for (var i = 0; i < sourceBounds.Length; i++)
            {
                var source = this.Sources[i];
                var sb = sourceBounds[i];
                foreach (var columnKvPair in this.gridData)
                {
                    if (!sb.IsInX(columnKvPair.Key))
                    {
                        continue;
                    }

                    foreach (var rowKvPair in columnKvPair.Value.Rows)
                    {
                        if (!sb.IsInY(rowKvPair.Key))
                        {
                            continue;
                        }

                        foreach (var cellKvPair in rowKvPair.Value.Cells)
                        {
                            if (!sb.IsInZ(cellKvPair.Key))
                            {
                                continue;
                            }

                            var cell = cellKvPair.Value;

                            // Get the cell position (center point of cell)
                            var cellPos = new Vector3(
                                relativeGridStart.x + (cell.Parent.Parent.Index * this.cellSize.x),
                                relativeGridStart.y + (cell.Parent.Index * this.cellSize.y),
                                relativeGridStart.z + (cell.Index * this.cellSize.z));

                            var dist = source.GetDistanceToCell(cellPos);
                            var newLevel = source.GetDistanceLevel(dist);

                            // When the cell distance is to large, don't do anything
                            if (newLevel >= PriorityLevelAmount)
                            {
                                continue;
                            }

                            // When this is the first source for the cell
                            if (cell.SourcesInRange.Count == 0)
                            {
                                // Remove the cell from the old list
                                if (cell.PriorityLevel < PriorityLevelAmount)
                                {
                                    this.cellPrioritySets[cell.PriorityLevel].Remove(cell);
                                }

                                cell.PriorityLevel = newLevel;

                                // Add the cell to the new list
                                cellsWithPriorityLevel.Add(cell);
                            }
                            else if (cell.PriorityLevel > newLevel)
                            {
                                cell.PriorityLevel = newLevel;
                            }

                            cell.SourcesInRange.Add(source);
                        }
                    }
                }
            }

            // Hide all old cells
            foreach (var cellPrioritySet in this.cellPrioritySets)
            {
                foreach (var cell in cellPrioritySet)
                {
                    this.RegisterCellForHiding(cell);
                }
            }

            // Reinitialize the priority sets
            for (int i = 0; i < this.cellPrioritySets.Length; i++)
            {
                this.cellPrioritySets[i] = new HashSet<GridCell>();
            }

            // Fill the cell levels with the new cells
            foreach (var cell in cellsWithPriorityLevel)
            {
                this.cellPrioritySets[cell.PriorityLevel].Add(cell);
                
                if (cell.IsMarkedForHiding)
                {
                    this.cellsToDeactivate.Remove(cell);
                    cell.IsMarkedForHiding = false;
                }
            }
            
            // Recalculate the updates amount for each priority
            this.UpdateCellUpF();
        }

        /// <summary>
        /// Distributes the updates per frame to the cells, depending on their priority
        /// </summary>
        /// <remarks>
        /// The curve used for the distribution of the calculation budget of the priority levels is f(x)=x^1.414<br/>
        /// This fits nicely with the perspective view falloff of a normal camera
        /// </remarks>
        private void UpdateCellUpF()
        {
            // Get the amount of active cells
            var cellAmount = this.cellPrioritySets.Sum(t => t.Count);

            // Get the multiplied amount of cells
            var multipliedAmount = 0;
            for (var i = 0; i < this.cellPrioritySets.Length; i++)
            {
                multipliedAmount += (int)(Mathf.Pow(this.cellPrioritySets.Length - i, 1.414f) * this.cellPrioritySets[i].Count);
            }

            if (multipliedAmount == 0)
            {
                multipliedAmount = 1;
            }

            // Get the average updates
            var averageUpdates = Math.Max((this.UpdatesPerFrame - cellAmount) / multipliedAmount, 0);
            
            // Apply the new updates per frame for each priority
            for (var i = 0; i < this.cellPrioritySets.Length; i++)
            {
                this.cellPriorityUpFs[i] = (int)Mathf.Max((Mathf.Pow(this.cellPrioritySets.Length - i, 1.414f) * averageUpdates) + 1.5f, 1);
            }
        }

        /// <summary>
        /// Updates the grid
        /// </summary>
        private void UpdateGrid()
        {
            // Get the amount of cells on each axis
            this.columnAmount = (int)Math.Ceiling(Math.Round(this.gridSize.x / this.cellSize.x, 6));
            this.rowAmount = (int)Math.Ceiling(Math.Round(this.gridSize.y / this.cellSize.y, 6));
            this.cellAmount = (int)Math.Ceiling(Math.Round(this.gridSize.z / this.cellSize.z, 6));

            this.gridData = new Dictionary<int, GridColumn>(this.columnAmount);
            this.cellsToDeactivate = new List<GridCell>();
            this.amountOfUsedCells = 0;

            var allObjects = FindObjectsOfType(typeof(LodObjectBase)) as LodObjectBase[];

            // Assign the objects to the cells
            if (allObjects != null)
            {
                foreach (var obj in allObjects)
                {
                    obj.ParentCell = null;
                    obj.Register();
                }
            }

            // Gets the amount of cells to display it
            this.AmountOfCells = this.columnAmount * this.rowAmount * this.cellAmount;

            // Update the priority of the cells
            this.UpdateCellLevels();
        }

        /// <summary>
        /// Checks if the cell size is below the minimum, prevents out of memory exceptions
        /// </summary>
        private void CheckCellSize()
        {
            // Apply minimum values
            if (this.cellSize.x < MinCellSize)
            {
                this.cellSize.x = MinCellSize;
            }

            if (this.cellSize.y < MinCellSize)
            {
                this.cellSize.y = MinCellSize;
            }

            if (this.cellSize.z < MinCellSize)
            {
                this.cellSize.z = MinCellSize;
            }

            // The maximum amount of cells is 1'000'000
            if (this.cellSize.x < this.gridSize.x / MaxCellsPerAxis)
            {
                this.cellSize.x = this.gridSize.x / MaxCellsPerAxis;
            }

            if (this.cellSize.y < this.gridSize.y / MaxCellsPerAxis)
            {
                this.cellSize.y = this.gridSize.y / MaxCellsPerAxis;
            }

            if (this.cellSize.z < this.gridSize.z / MaxCellsPerAxis)
            {
                this.cellSize.z = this.gridSize.z / MaxCellsPerAxis;
            }
        }

        /// <summary>
        /// Registers the cell for content hiding
        /// </summary>
        /// <param name="cell">The cell.</param>
        private void RegisterCellForHiding(GridCell cell)
        {
            if (cell.IsMarkedForHiding)
            {
                return;
            }

            this.cellsToDeactivate.Add(cell);
            cell.IsMarkedForHiding = true;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Gets called when the gizmos is active and the object is selected<br/>
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // Only draw when the draw when selected flas is false and the manager is activated
            if (!this.drawWhenDeselected)
            {
                this.DrawGizmos();
            }
        }

        /// <summary>
        /// Gets called when the gizmos is active<br/>
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        private void OnDrawGizmos()
        {
            // Gizmos drawn in this method can prevent the user from selecting objects behind the gizmos
            // So use this method only when needed
            if (this.drawWhenDeselected)
            {
                this.DrawGizmos();
            }

            // Clears the list that contains all drawn cells
            this.usedCells = new HashSet<GridCell>();
        }

        /// <summary>
        /// Draws the gizmos for all visualization settings<br/>
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        private void DrawGizmos()
        {
            if ((Application.isPlaying && !this.drawInDebug) || !this.enabled)
            {
                return;
            }

            var halfCellSize = this.cellSize / 2f;

            // Draws the grid (all active and inactive cells)
            if (this.drawGridGizmos)
            {
                Gizmos.color = new Color(0, 0, 0, 0.5f);
                var startOffset = new Vector3(
                    this.gridStart.x + this.cellSize.x / 2f,
                    this.gridStart.y + this.cellSize.y / 2f,
                    this.gridStart.z + this.cellSize.z / 2f);
                var size = new Vector3(
                    Mathf.Max(this.columnAmount * this.cellSize.x, this.gridSize.x),
                    Mathf.Max(this.rowAmount * this.cellSize.y, this.gridSize.y),
                    Mathf.Max(this.cellAmount * this.cellSize.z, this.gridSize.z));
                var bigCenter = this.gridStart + (size / 2f);

                //Gizmos.DrawWireCube(bigCenter, this.gridSize);

                for (var x = 0; x < this.columnAmount && x < MaxCellsPerAxis; x++)
                {
                    var currentCenter = new Vector3(startOffset.x + x * this.cellSize.x, bigCenter.y, bigCenter.z);
                    Gizmos.DrawWireCube(currentCenter, new Vector3(this.cellSize.x, size.y, size.z));
                }

                for (var y = 0; y < this.rowAmount && y < MaxCellsPerAxis; y++)
                {
                    var currentCenter = new Vector3(bigCenter.x, startOffset.y + y * this.cellSize.y, bigCenter.z);
                    Gizmos.DrawWireCube(currentCenter, new Vector3(size.x, this.cellSize.y, size.z));
                }

                for (var z = 0; z < this.cellAmount && z < MaxCellsPerAxis; z++)
                {
                    var currentCenter = new Vector3(bigCenter.x, bigCenter.y, startOffset.z + z * this.cellSize.z);
                    Gizmos.DrawWireCube(currentCenter, new Vector3(size.x, size.y, this.cellSize.z));
                }
            }

            // Draws all cell levels
            if (this.drawCellLevelGizmos)
            {
                for (var i = 0; i < this.cellPrioritySets.Length; i++)
                {
                    foreach (var cell in this.cellPrioritySets[i])
                    {
                        var center =
                                new Vector3(
                                    this.gridStart.x + (cell.Parent.Parent.Index * this.cellSize.x) + halfCellSize.x,
                                    this.gridStart.y + (cell.Parent.Index * this.cellSize.y) + halfCellSize.y,
                                    this.gridStart.z + (cell.Index * this.cellSize.z) + halfCellSize.z);


                        var relativePos = (float)(i) / (this.cellPrioritySets.Length - 1);
                        Gizmos.color = new Color(Mathf.Min(2 * relativePos, 1), Mathf.Min(2 * (1 - relativePos), 1), 0, 0.3f);
                        Gizmos.DrawCube(center, this.cellSize);
                    }
                }
            }

            // Draws all used cells
            if (this.drawCellGizmos)
            {
                Gizmos.color = Color.cyan;

                foreach (var columnKvPair in this.gridData)
                {
                    foreach (var rowKvPair in columnKvPair.Value.Rows)
                    {
                        foreach (var cellKvPair in rowKvPair.Value.Cells)
                        {
                            var cell = cellKvPair.Value;
                            var center =
                                new Vector3(
                                    this.gridStart.x + (cell.Parent.Parent.Index * this.cellSize.x) + halfCellSize.x,
                                    this.gridStart.y + (cell.Parent.Index * this.cellSize.y) + halfCellSize.y,
                                    this.gridStart.z + (cell.Index * this.cellSize.z) + halfCellSize.z);

                            // Draws all active cells over the grid
                            Gizmos.DrawWireCube(center, this.cellSize);
                        }
                    }
                }
            }

            // Draws all selected objects over all other gizmos
            if (this.drawObjectCellGizmos)
            {
                foreach (var cell in this.usedCells)
                {
                    var center =
                        new Vector3(
                            this.gridStart.x + (cell.Parent.Parent.Index * this.cellSize.x) + halfCellSize.x,
                            this.gridStart.y + (cell.Parent.Index * this.cellSize.y) + halfCellSize.y,
                            this.gridStart.z + (cell.Index * this.cellSize.z) + halfCellSize.z);

                    Gizmos.color = new Color(0, 0, 1);
                    Gizmos.DrawWireCube(center, this.cellSize);
                }
            }
        }

        /// <summary>
        /// Draws all child objects of a given game object<br/>
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        /// <param name="rootObject">
        /// The root object.
        /// </param>
        private void DrawChildObjects(GameObject rootObject)
        {
            if (!this.drawDisabledObjects && !rootObject.activeInHierarchy)
            {
                return;
            }

            var meshFilter = rootObject.GetComponent<MeshFilter>();
            var skinnedMeshFilter = rootObject.GetComponent<SkinnedMeshRenderer>();
            var t = rootObject.transform;
            
            if (meshFilter != null)
            {
                Graphics.DrawMesh(
                    meshFilter.sharedMesh, Matrix4x4.TRS(t.position, t.rotation, t.lossyScale), this.managedObjectsMaterial, 0);
            }

            if (skinnedMeshFilter != null)
            {
                Graphics.DrawMesh(
                    skinnedMeshFilter.sharedMesh, Matrix4x4.TRS(t.position, t.rotation, t.lossyScale), this.managedObjectsMaterial, 0);
            }

            for (var i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                this.DrawChildObjects(child.gameObject);
            }
        }

        /// <summary>
        /// Gets called when the object is instantiated or the gui has a value change<br/>
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        private new void OnValidate()
        {
            base.OnValidate();

            // Check if the grid start has changed and update when necessary
            if (this.prevGSt != this.gridStart)
            {
                this.prevGSt = this.gridStart;
                this.UpdateGrid();
                this.HasSourceUpdates = true;
            }

            // Check if the grid size has changed and update when necessary
            if (this.prevGSi != this.gridSize)
            {
                // Limit the minimum size for each axis
                if (this.gridSize.x < MinGridSize)
                {
                    this.gridSize.x = MinGridSize;
                }

                if (this.gridSize.y < MinGridSize)
                {
                    this.gridSize.y = MinGridSize;
                }

                if (this.gridSize.z < MinGridSize)
                {
                    this.gridSize.z = MinGridSize;
                }

                // Update the cell size when necessary
                this.CheckCellSize();

                this.prevGSi = this.gridSize;
                this.UpdateGrid();
                this.HasSourceUpdates = true;
            }

            // Check if the cell size has been changed
            if (this.prevCS != this.cellSize)
            {
                this.CheckCellSize();

                this.prevCS = this.cellSize;
                this.UpdateGrid();
                this.HasSourceUpdates = true;
            }

            if (this.clampObjectsToGrid != this.prevCOTG)
            {
                this.prevCOTG = this.clampObjectsToGrid;
                this.UpdateGrid();
            }

            // Limit the max cell deactivations to at least one
            if (this.maxCellDeactivations < 1)
            {
                this.maxCellDeactivations = 1;
            }
        }

#endif

        #endregion
    }
}