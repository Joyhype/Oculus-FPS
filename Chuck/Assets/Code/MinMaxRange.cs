[System.Serializable]
public class MinMaxRange {
	public float Min;
	public float Max;

	public float GetValue() {
		return UnityEngine.Random.Range( Min, Max );
	}
}
