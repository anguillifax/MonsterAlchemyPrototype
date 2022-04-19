// namespace GameJam.Systemic

// Public API for art/sound.
// Rename to `EntityCallback`.
public class EntityInterface {
	public UnityEngine.Events.Event BurnMultiplierDecreased;
}



// Data-only type.
// Unique per spawned entity.
// Only visible to RuleBook and Designer
[Serializable] public class EntityState {

	// Outward facing API.
	// Reference only, do not serialize.
	public EntityInterface Out { get; set; }

	// Native API
	public int HealthAmount;
	public int WaterAmount;
	public bool Flammable;
	public bool OnFire;
	public int BurningMult;
	public float BurningTimer;
	public bool Immobilized;

	// Queries (Optional)
	public bool HasHealth => healthAmount > 0;
	public bool HasFlammable => flammable;
	public int BurningCount => burning;

	// Sets valid defaults.
	public EntityState() {
		HealthAmount = 0;
		WaterAmount = 0;
		Flammable = false;
		OnFire = false;
		// etc.

		Validator.ValidateAndThrow(this);
	}

	// Deep copy.
	public EntityState(EntityState o) {
		Validator.ValidateAndThrow(o);

		this.Health = o.Health;
		this.Water = o.Water;
		// etc.
	}
}



// Checks if EntityState values are legal.
// Used both in-game and in-editor.
public static class Validator {
	public struct Response {
		public bool Valid { get; set; }
		public string ErrorMessage { get; set; }
	}

	public static Response Validate(EntityState);
	public static void ValidateAndThrow(EntityState);

	private static string ValidateInner(EntityState s) {
		if (s.WaterAmount < 0) {
			return $"Water must be >= 0. Is {s.WaterAmount}.";
		}
		return null;
	}
}



// Manipulates EntityState.
// Only one copy per game.
public class RulebookAsset : ScriptableObject {

	// Tuning data.
	public float BurnTimerMax = 2.0f;

	// Called by each entity on FixedUpdate().
	public void Update(EntityState o) {
		// Burn Timer
		// "object" -> "o"

		// Big ol' list of rules.
		if (o.BurnMultiplier > 0) {
			o.BurningTimer -= Time.fixedDelta;
			if (o.BurningTimer <= 0) {
				o.BurnMultiplier -= 1;
				o.Out.BurnMultiplierDecrease.Invoke();
			}
		} else {
			o.BurnTimer = BurnTimerMax;
		}

		// Sanity check.
		if (debugMode) {
			Validator.Validate(o);
		}
	}

	// Called twice when two objects collide as (a,b) (b,a).
	public void ContactBegin(EntityState s, EntityState t, Collision2D col) {
		// "source" -> "s"
		// "target" -> "t"
		
		// Big ol' list of rules.
		if (s.Burnable) {
			t.BurnMultiplier += 1;
		}

		// Sanity check.
		if (debugMode) {
			Validator.Validate(s);
			Validator.Validate(t);
		}
	}

}



// Provides initial values.
// Usually unique per prefab. Possibly shared between prefabs.
public class EntityAsset : ScriptableObject {
	public EntityState State;
	// EntityInterface stored on prefab.
}



// Base type for all systemic objects.
public class SystemicEntity : MonoBehaviour {
	public EntityAsset initialState;
	public RulebookAsset rulebook;
	public EntityState state;
	public EntityInterface callbacks;

	void Awake() {
		state = new EntityState(initialState.State);
		state.Out = callbacks;
	}

	void Update() {
		rulebook.Update(state);
	}

	void OnTriggerEnter2D(Collider2D col) {
		var script = col.GetComponent<SystemicEntity>();
		if (script) {
			rulebook.ContactBegin(state, script.state, null);
		}
	}
}
