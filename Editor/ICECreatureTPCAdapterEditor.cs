// ##############################################################################
//
// ICECreatureTPCAdapterEditor.cs
// Version 1.1
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
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AnimatedValues;

using ICE;
using ICE.World;
using ICE.World.Utilities;
using ICE.World.EditorUtilities;
using ICE.Shared;

using ICE.Creatures;
using ICE.Creatures.Utilities;
using ICE.Creatures.Objects;
using ICE.Creatures.EnumTypes;
using ICE.Creatures.EditorInfos;
using ICE.Creatures.EditorUtilities;

namespace ICE.Creatures.Adapter
{

	[CustomEditor(typeof(ICECreatureTPCAdapter))]
	public class ICECreatureTPCAdapterEditor : Editor
	{
		private string _damage_behaviour;
		//private vp_DamageInfo.DamageType _damage_type;
		public override void OnInspectorGUI()
		{
			ICECreatureTPCAdapter _adapter = (ICECreatureTPCAdapter)target;
			ICECreatureControl _control = _adapter.GetComponent<ICECreatureControl>();	

			EditorGUILayout.Separator();
			_adapter.UseCreatureDamage = ICEEditorLayout.ToggleLeft( "Creature Damage", "", _adapter.UseCreatureDamage, true );
			if( _adapter.UseCreatureDamage )
			{
				EditorGUI.indentLevel++;
				_adapter.UseAdvanced = ICEEditorLayout.ToggleLeft( "Use Advanced", "", _adapter.UseAdvanced, true );
				if( _adapter.UseAdvanced )
					CreatureObjectEditor.DrawInfluenceDataObject( _adapter.Influences, EditorHeaderType.FOLDOUT_ENABLED_BOLD, _control.Creature.Status.UseAdvanced );
				EditorGUI.indentLevel--;
				EditorGUILayout.Separator();
			}

			_adapter.UsePlayerDamage = ICEEditorLayout.ToggleLeft( "Player Damage", "", _adapter.UsePlayerDamage, true );
			if( _adapter.UsePlayerDamage )
			{
				EditorGUI.indentLevel++;

				_adapter.UseMultiplePlayerDamageHandler = ICEEditorLayout.Toggle( "Use Multiple Damage Handler", "", _adapter.UseMultiplePlayerDamageHandler, "" );
				if( _adapter.UseMultiplePlayerDamageHandler )
				{
					foreach( ICECreaturePlayerDamageObject _damage in _adapter.PlayerDamages )
					{
						ICEEditorLayout.BeginHorizontal();
						ICEEditorLayout.Label( _damage.DamageBehaviourModeKey , true );
						GUILayout.FlexibleSpace();
						if( GUILayout.Button( new GUIContent( "X", "Delete" ), ICEEditorStyle.CMDButton ) )
						{
							_adapter.PlayerDamages.Remove( _damage );
							return;
						}
						ICEEditorLayout.EndHorizontal();

						DrawPlayerDamage( _control, _damage );
					}

					ICEEditorStyle.SplitterByIndent(EditorGUI.indentLevel + 1);
					ICEEditorLayout.BeginHorizontal();
					_damage_behaviour = Popups.BehaviourPopup( _control, _damage_behaviour );

					EditorGUI.BeginDisabledGroup( _damage_behaviour.Trim() == "" );
					if( GUILayout.Button( new GUIContent( "ADD", "Adds a new damage handler" ), ICEEditorStyle.CMDButtonDouble ) )
						_adapter.AddPlayerDamage( _damage_behaviour );
					EditorGUI.EndDisabledGroup();
					ICEEditorLayout.EndHorizontal();
				}
				else
				{
					DrawPlayerDamage( _control, _adapter.SimpleDamage );
				}

				EditorGUI.indentLevel--;
			}

			EditorGUI.indentLevel++;
			EditorGUILayout.Separator();
		}

		private void DrawPlayerDamage( ICECreatureControl _control, ICECreaturePlayerDamageObject _damage )
		{
			EditorGUI.indentLevel++;
				_damage.Damage = ICEEditorLayout.Slider( "Damage", "", _damage.Damage, 0.05f, 0, 100 );
				//_damage.Type =  (DamageType)ICEEditorLayout.EnumPopup( "Damage Type", "", _damage.Type );
				EditorGUILayout.Separator();
				_damage.DamageBehaviourModeKey = BehaviourEditor.BehaviourSelect( _control, "Trigger Behaviour", "", _damage.DamageBehaviourModeKey , "ATTACK" );

				_damage.Range = ICEEditorLayout.MaxDefaultSlider( "Trigger Range", "", _damage.Range, 0.05f, 0, ref _damage.RangeMax, 2, ""  );
				ICEEditorLayout.MinMaxGroup( "Trigger Interval", "", ref _damage.IntervalMin, ref _damage.IntervalMax, 0,60, 0.05f, "" );
				ICEEditorLayout.MinMaxGroup( "Trigger Interruption Interval", "", ref _damage.InterruptionIntervalMin, ref _damage.InterruptionIntervalMax, 0,60, 0.05f, "" );
				ICEEditorLayout.MinMaxGroup( "Trigger Limiter", "", ref _damage.LimitMin, ref _damage.LimitMax, 0,60, 1, "" );
				_damage.Force = ICEEditorLayout.DefaultSlider( "Force", "", _damage.Force, 0.25f, 0, 100, 20.0f, "" );
				_damage.MuzzleFlash = (Renderer)EditorGUILayout.ObjectField( "Muzzle Flash", _damage.MuzzleFlash, typeof(Renderer), true );
				_damage.Sound = (AudioClip)EditorGUILayout.ObjectField( "Fire Sound", _damage.Sound, typeof(AudioClip), false );
				_damage.FxRandonPitch = ICEEditorLayout.DefaultSlider( "Fire Fx Randon Pitch", "", _damage.FxRandonPitch, 0.001f, -1, 1, 0.86f, "" );
			EditorGUI.indentLevel--;
		}
	}

}