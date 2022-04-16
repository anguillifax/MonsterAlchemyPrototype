// namespace GameJam.Systemic



// Data-only shared type.
[Serializable] public class EntityState {

	public int healthAmount;
	public int waterAmount;
	public bool flammable;
	public bool onFire;
	public int burningMult;
	public float burningTimer;
	public bool immobilize

	// Sets valid defaults.
	public EntityState() {
		healthAmount = 0;
		waterAmount = 0;
		flammable = false;
		onFire = false;
		// etc.

		Validator.ValidateAndThrow(this);
	}

	// Deep copy.
	public EntityState(EntityState o) {
		Validator.ValidateAndThrow(o);

		this.health = o.health;
		this.water = o.water;
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
		if (s.waterAmount < 0) {
			return $"Water must be >= 0. Is {s.waterAmount}.";
		}
		return null;
	}
}



// Provides initial values.
public class EntityAsset : ScriptableObject {

	// Discourage reassignment.
	[SerializeField] private EntityState _state;
	public EntityState State => _state;

	// Deep copy for use at runtime.
	public EntityState CopyState() => new EntityState(_state);

}



// Exposes mutable configuration for art/sound.
public class EntityCustomization {
	public UnityEngine.Events.Event burnMultiplierDecrease;
}



// Reads EntityState and executes tag behavior.
// Each effect ("tag executor") is a singleton in the asset database.
public abstract class Effect {
	public EntityState State { get; set; }
	public EntityCustomization Customization { get; set; }
	public abstract void Update();
}

// Example effect.
[Serializable] public class EffectBurning : Effect {
	public override void Update() {
		if (State.burnMultiplier > 0) {
			State.burningTimer -= Time.fixedDelta;
			if (burningTimer <= 0) {
				State.burnMultiplier -= 1;
				Customization.burnMultiplierDecrease.Invoke();
			}
		}
	}
}



// Singleton.
public class EffectsAsset : ScriptableObject {

	// Explicitly list out effects.
	public EffectFlammable flammable;
	public EffectBurning burning;
	// etc.

	// Specify execution order.
	public Effect[] Effects => new Effect[] {
		flammable,
		burning,
	};
}



// Base type for all systemic objects.
public class SystemicEntity : MonoBehaviour {
	public EntityAsset initialState;
	public EffectsAsset effects;
	public EntityCustomization customization;
	public EntityState state;

	void Awake() {
		state = initialState.CopyState();

		foreach (Effect e in effects.Effects) {
			e.State = state;
			e.Customization = customization;
		}
	}

	void Update() {
		Validator.ValidateAndThrow(state);
		foreach (Effect e in effects.Effects) {
			e.Update();
		}
	}
}


