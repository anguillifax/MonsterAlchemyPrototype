struct SystemicEntityStat {
	public int Water;
	public int Fire;
	public int Health;
}

class SystemicEntity {
	public SystemicEntityStat Stat;
	public Tag[] GetTags<T>() : where T is Tag {
		return GetComponents<T>();
	};
}

class Tag : MonoBehaviour {
}

class Frozen : Tag {}