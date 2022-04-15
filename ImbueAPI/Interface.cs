struct Effector {
	GameObject sender;
	Mixture mixture;
}

class Mixture {
	public int this[Aspect aspect] { ... }
}

// Execution Order < 0
class Reagent : Monobehaviour {
	private List<Effector> current;
	private List<Effector> next;

	public List<Effector> Current => current;

	public void Imbue(GameObject sender, Mixture mixture) => Imbue(new Effector() {sender, mixture});
	public void Imbue(Effector effector) {
		next.Add(effector);
	}

	protected virtual void Update() {
		current = next;
		next = new List<Effector>();
	}
}


// Example

class Torch : Monobehaviour {
	private reagent;

	void Awake() {
		reagent = GetComponent<Reagent>();
	}

	void Update() {
		foreach (Mixture m in reagent.Current) {
			// do the thing
		}
	}
}