// namespace GameJam.Systemic

// Data-only shared type.
// Unique per spawned entity.
[Serializable] public class EntityState {

	// Native API
	public int healthAmount;
	public int waterAmount;
	public bool flammable;
	public bool onFire;
	public int burningMult;
	public float burningTimer;
	public bool immobilize

	// Queries
	public bool HasHealth => healthAmount > 0;
	public bool HasFlammable => flammable;
	public int BurningCount => burning;

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
// Unique per prefab. Possibly shared between prefabs.
public class EntityAsset : ScriptableObject {

	// Discourage reassignment.
	[SerializeField] private EntityState _state;
	public EntityState State => _state;

	// Deep copy for use at runtime.
	public EntityState CopyState() => new EntityState(_state);

}



// Exposes mutable configuration for art/sound.
// Unique per instance.
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



// Allows swapping out effects lists for different game modes / testing.
public interface IEffectList {
	// Specify execution order.
	Effect[] Effects { get; }
}

// Singleton.
public class EffectListAsset : ScriptableObject, IEffectList {

	// Enables configuration in-editor.
	public EffectFlammable flammable;
	public EffectBurning burning;
	// etc.

	private Effect[] cache;

	Effect[] IEffectList.Effects {
		get {
			// Lazy initialization
			if (cache == null) {
				cache = new Effect[] {
					flammable,
					burning,
				};
				// #if UNITY_EDITOR, warn if unincluded variables.
			}

			return cache;
		}
	}
	
}



// Base type for all systemic objects.
public class SystemicEntity : MonoBehaviour {
	public EntityAsset initialState;
	public EffectListAsset effectListAsset;
	public EntityCustomization customization;
	public EntityState state;
	public IEffectList effects;

	void Awake() {
		state = initialState.CopyState();

		effects = (IEffectList)effectListAsset;

		foreach (Effect e in effects.Effects) {
			e.State = state;
			e.Customization = customization;
		}
	}

	void Update() {
		Validator.ValidateAndThrow(state);
		foreach (Effect e in effects.Effects) {
			e.Update();
			// if AggressiveDebug -> Validate(state); on error, log e.GetType().Name;
		}
	}
}


