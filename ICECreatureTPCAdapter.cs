// ##############################################################################
//
// ICECreatureTPCAdapter.cs
// Version 1.1.2
//
// © Pit Vetterick, ICE Technologies Consulting LTD. All Rights Reserved.
// http://www.ice-technologies.com
// mailto:support@ice-technologies.com
// 
// Unity Asset Store End User License Agreement (EULA)
// http://unity3d.com/legal/as_terms
//
// ##############################################################################

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ICE.Creatures;
using ICE.Creatures.Objects;
using Opsive.ThirdPersonController;


namespace ICE.Creatures.Adapter
{
	[System.Serializable]
	public class ICECreaturePlayerDamageObject : System.Object 
	{
		public ICECreaturePlayerDamageObject(){}
		public ICECreaturePlayerDamageObject( string _behaviour )
		{
			DamageBehaviourModeKey = _behaviour;
		}

		public string DamageBehaviourModeKey = "";
		public float Damage = 1;
		public float RangeMax = 100;
		public float Range = 2;
		public float IntervalMin = 0.09f;
		public float IntervalMax = 0.09f;
		public float LimitMin = 3;
		public float LimitMax = 6;
		public float InterruptionIntervalMin = 2f;
		public float InterruptionIntervalMax = 6f;
		//public DamageType Type = DamageType.UNDEFINED;
		public float Force = 20.0f;			
		public Renderer MuzzleFlash;		
		public AudioClip Sound;
		public float FxRandonPitch = 0.86f;

		private float m_PlayerDamageEffectTimer = 0;
		public float PlayerDamageEffectTime = 0.3f;

		private float m_PlayerDamageTimer = 0;
		private float m_PlayerDamageInterval = 0.09f;
		private int m_PlayerDamageCounter = 0;
		private int m_PlayerDamageLimit = 3;
		private float m_PlayerDamageInterruptionTimer = 0;
		private float m_PlayerDamageInterruptionTime = 0.5f;

		public void Init ()
		{
			m_PlayerDamageInterval = 0;//Random.Range( IntervalMin, IntervalMax );
			m_PlayerDamageInterruptionTime = Random.Range( InterruptionIntervalMin, InterruptionIntervalMax );
			m_PlayerDamageLimit = (int)Random.Range( LimitMin, LimitMax );

			if( MuzzleFlash )
				MuzzleFlash.enabled = false;
		}

		public void Reset ()
		{
			m_PlayerDamageInterval = Random.Range( IntervalMin, IntervalMax );
			m_PlayerDamageInterruptionTime = Random.Range( InterruptionIntervalMin, InterruptionIntervalMax );
			m_PlayerDamageLimit = (int)Random.Range( LimitMin, LimitMax );

			if( MuzzleFlash )
				MuzzleFlash.enabled = false;
		}

		public void Update ()
		{
			if( MuzzleFlash )
			{
				m_PlayerDamageEffectTimer += Time.deltaTime;
				if( m_PlayerDamageEffectTimer > PlayerDamageEffectTime )
					MuzzleFlash.enabled = false;
			}
		}

		public bool IsActive( ICECreatureControl _control )
		{
			if( DamageBehaviourModeKey != "" && 
				_control.Creature.Behaviour.ActiveBehaviourModeKey == DamageBehaviourModeKey &&
				_control.Creature.ActiveTargetMovePositionDistance <= Range )
				return true;
			else
				return false;
		}

		public bool IsReady( ICECreatureControl _control )
		{
			if( _control == null || _control.Creature == null || _control.Creature.Behaviour == null )
				return false;

			bool _ready = false;

			if( IsActive( _control ) )
			{
				if( m_PlayerDamageCounter < m_PlayerDamageLimit || Mathf.Max( LimitMin, LimitMax ) == 0 )
				{
					m_PlayerDamageTimer += Time.deltaTime;
					if( m_PlayerDamageTimer >= m_PlayerDamageInterval )
					{
						_ready = true;
						m_PlayerDamageCounter += 1;
						m_PlayerDamageTimer = 0;
						m_PlayerDamageInterval = Random.Range( IntervalMin, IntervalMax );

						if( MuzzleFlash )
						{
							m_PlayerDamageEffectTimer = 0;
							PlayerDamageEffectTime = m_PlayerDamageInterval / 3;
							MuzzleFlash.enabled = true;
						}
					}

					m_PlayerDamageInterruptionTime = Random.Range( InterruptionIntervalMin, InterruptionIntervalMax );
				}
				else
				{
					m_PlayerDamageInterruptionTimer += Time.deltaTime; 
					if( m_PlayerDamageInterruptionTimer > m_PlayerDamageInterruptionTime || Mathf.Max( InterruptionIntervalMin, InterruptionIntervalMax ) == 0 )
					{
						m_PlayerDamageCounter = 0;
						m_PlayerDamageInterruptionTimer = 0;
						m_PlayerDamageLimit = (int)Random.Range( LimitMin, LimitMax );
					}
				}

			}
			else
				Reset();

			return _ready;
		}
	}


	[RequireComponent (typeof (ICECreatureControl))]
	public class ICECreatureTPCAdapter : Health 
	{
		public List<ICECreaturePlayerDamageObject> PlayerDamages = new List<ICECreaturePlayerDamageObject>();

		public bool UseTPCHealth = false;
		public bool UseCreatureDamage = true;
		public bool UsePlayerDamage = true;
		public bool UseAdvanced = true;
		public string BehaviourModeKey = "";
		public bool UseMultiplePlayerDamageHandler = false;

		[SerializeField]
		private InfluenceDataObject m_Influences = null;
		public virtual InfluenceDataObject Influences{
			get{ return m_Influences = ( m_Influences == null ? new InfluenceDataObject():m_Influences ); }
			set{ Influences.Copy( value ); }
		}


		private ICECreatureControl m_Controller = null;
		private ICECreaturePlayerDamageObject m_ActiveDamage = null;
		public ICECreaturePlayerDamageObject SimpleDamage = new ICECreaturePlayerDamageObject();

		protected override void Awake()
		{
			m_Controller = GetComponent<ICECreatureControl>();

			if( UseTPCHealth )
				base.Awake();

		}

		public void Update ()
		{
			if( ! UsePlayerDamage )
				return;

			ICECreaturePlayerDamageObject _damage = GetPlayerDamage();

			if( _damage != null )
			{
				if( m_ActiveDamage != _damage )
				{
					m_ActiveDamage = _damage;
					m_ActiveDamage.Init();
				}
			}
			else if( m_ActiveDamage != null )
			{
				m_ActiveDamage.Reset();				
				m_ActiveDamage = null;
			}

			if( m_ActiveDamage != null )
			{
				if( m_ActiveDamage.IsReady( m_Controller ) )
					HandlePlayerDamage( m_ActiveDamage );

				m_ActiveDamage.Update();
			}


		}

		public override void Damage( float amount, Vector3 position, Vector3 force, float radius, GameObject attacker, GameObject hitGameObject )
		{
			ApplyDamage( amount, position, force, radius, attacker );
			if( UseTPCHealth )
				base.Damage(amount, position, force, attacker);
		}

		private void ApplyDamage( float amount, Vector3 position, Vector3 force, float radius, GameObject attacker )
		{
			
			if( ! UseCreatureDamage || m_Controller == null )
				return;

			if( UseAdvanced )
			{
				m_Controller.Creature.UpdateStatusInfluences( Influences );
			}
			else
			{
				m_Controller.Creature.Status.AddDamage( amount );
			}

			if( BehaviourModeKey != "" && m_Controller.Creature.Status.IsDead == false )
				m_Controller.Creature.Behaviour.SetBehaviourModeByKey( BehaviourModeKey );
		}

		private void HandlePlayerDamage( ICECreaturePlayerDamageObject _damage )
		{
			if( m_Controller == null || m_Controller.Creature.ActiveTarget == null || m_Controller.Creature.ActiveTarget.TargetGameObject == null )
				return;

			Health _health = null;
			if( ( _health = m_Controller.Creature.ActiveTarget.TargetGameObject.GetComponentInParent<Health>() ) != null )
			{
				_health.Damage( _damage.Damage, m_Controller.gameObject.transform.position, - m_Controller.gameObject.transform.forward * _damage.Force, m_Controller.gameObject );
			}
		}

		public bool AddPlayerDamage( string _behaviour )
		{
			if( _behaviour.Trim() == "" )
				return false;

			PlayerDamages.Add( new ICECreaturePlayerDamageObject( _behaviour ) );
			return true;
		}

		private ICECreaturePlayerDamageObject GetPlayerDamage()
		{
			if( PlayerDamages.Count > 0 )
			{
				List<ICECreaturePlayerDamageObject> _damages = new List<ICECreaturePlayerDamageObject>();
				foreach( ICECreaturePlayerDamageObject _damage in PlayerDamages )
				{
					if( _damage.IsActive( m_Controller ) )
						_damages.Add( _damage );
				}

				if( _damages.Count == 1 ){
					return _damages[0];
				}else if( _damages.Count > 1 ){
					return _damages[Random.Range(0,_damages.Count)];
				}else
					return null;
			}
			else if( SimpleDamage.IsActive( m_Controller ) )
				return SimpleDamage;
			else
				return null;
		}
	}
}
