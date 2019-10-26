﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    partial class Shaman
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass shaman_class;
        static public BlueprintProgression shaman_progression;
        static public BlueprintFeatureSelection shaman_spirits;
        static public BlueprintFeatureSelection wandering_shaman_spirits;
        static public BlueprintFeatureSelection hex_selection;
        static public BlueprintFeatureSelection wandering_hex_selection;
        static public BlueprintFeatureSelection witch_hex_selection;
        static public BlueprintFeatureSelection shaman_familiar;
        static public BlueprintFeature shaman_orisons;
        static public BlueprintFeature shaman_proficiencies;
        static public BlueprintSpellbook spirit_magic_spellbook;
        static public BlueprintFeature spirit_magic;

        static public Dictionary<string, Spirit> spirits = new Dictionary<string, Spirit>();

        static HexEngine hex_engine;
        //hexes
        static public BlueprintFeature healing;
        static public BlueprintFeature chant;
        static public BlueprintFeature misfortune_hex;
        static public BlueprintFeature fortune_hex;
        static public BlueprintFeature evil_eye;
        static public BlueprintFeature ward;
        static public BlueprintFeature shapeshift;
        static public BlueprintFeature wings_attack_hex;
        static public BlueprintFeature wings_hex;
        static public BlueprintFeature draconic_resilence;
        static public BlueprintFeature fury;
        static public BlueprintFeature secret;
        static public BlueprintFeature intimidating_display;
        //additional witch hexes
        static public BlueprintFeature beast_of_ill_omen;
        static public BlueprintFeature slumber_hex;
        static public BlueprintFeature iceplant_hex;
        static public BlueprintFeature murksight_hex;
        static public BlueprintFeature ameliorating;
        static public BlueprintFeature summer_heat;

        static public BlueprintFeature extra_hex;

        static public BlueprintArchetype overseer_archetype;
        static public BlueprintFeature controlling_magic;
        static public BlueprintSpellList controlling_magic_spell_list;

        static public BlueprintArchetype speaker_for_the_past_archetype;
        static public MysteryEngine mystery_engine;
        static public BlueprintFeatureSelection revelation_selection;
        static public BlueprintFeature mysteries_of_the_past;


        static public BlueprintArchetype witch_doctor_archetype;
        static public BlueprintFeature channel_energy;
        static public BlueprintFeature counter_curse;


        public class Spirit
        {
            public BlueprintProgression progression;
            public BlueprintProgression wandering_progression;
            public BlueprintFeatureSelection hex_selection;
            public BlueprintFeatureSelection wandering_hex_selection;
            public BlueprintSpellList spirit_magic_spell_list;
            public BlueprintFeature spirit_magic_spells;


            internal Spirit(string name, string display_name, string description, UnityEngine.Sprite icon, string guid,
                BlueprintFeature spirit_ability, BlueprintFeature greater_spirit_ability, BlueprintFeature true_spirit_ability, BlueprintFeature manifestation,
                BlueprintFeature[] hexes, BlueprintAbility[] spells)
                : this(name, display_name, description, icon, guid, new BlueprintFeature[] {spirit_ability, spirit_ability}, new BlueprintFeature[] { greater_spirit_ability, greater_spirit_ability },
                      new BlueprintFeature[] { true_spirit_ability, true_spirit_ability }, manifestation, hexes, spells)
            {

            }


            internal Spirit(string name, string display_name, string description, UnityEngine.Sprite icon, string guid,
                    BlueprintFeature[] spirit_ability, BlueprintFeature[] greater_spirit_ability, BlueprintFeature[] true_spirit_ability, BlueprintFeature manifestation, 
                    BlueprintFeature[] hexes, BlueprintAbility[] spells)
            {
                string spells_description = display_name + " spirit grants shaman the following spells: ";
                spirit_magic_spell_list = Common.createSpellList(name + "SpiritMagicSpellList", "");
                for (int i = 0; i < spells.Length; i++)
                {
                    spells[i].AddToSpellList(spirit_magic_spell_list, i + 1);
                    spells_description += spells[i].Name + ((i == (spells.Length - 1)) ? "" : ", ");
                }
                spells_description += ".";


                spirit_magic_spells = Helpers.CreateFeature(name + "SpiritSpellsFeature",
                                                            display_name + " Spirit Magic",
                                                            "",
                                                            "",
                                                            null,
                                                            FeatureGroup.None,
                                                            Helpers.Create<NewMechanics.LearnSpellListToSpecifiedSpellbook>(l => { l.spellbook = spirit_magic_spellbook;
                                                                                                                                   l.SpellList = spirit_magic_spell_list;})
                                                            );
                spirit_magic_spells.HideInCharacterSheetAndLevelUp = true;
                spirit_magic_spells.HideInUI = true;

                var entries = new LevelEntry[] { Helpers.LevelEntry(1, spirit_ability[0], spirit_magic_spells),
                                                 Helpers.LevelEntry(8, greater_spirit_ability[0]),
                                                 Helpers.LevelEntry(16, true_spirit_ability[0]),
                                                 Helpers.LevelEntry(20, manifestation)
                                               };

                var wandering_entries = new LevelEntry[] { Helpers.LevelEntry(4, spirit_ability[1]),
                                                 Helpers.LevelEntry(12, greater_spirit_ability[1]),
                                                 Helpers.LevelEntry(20, true_spirit_ability[1])
                                               };

                progression = Helpers.CreateProgression(name + "SpiritProgression",
                                                        display_name + " Spirit",
                                                        description + $"\n{spells_description}",
                                                        "",
                                                        icon,
                                                        FeatureGroup.None,
                                                        Common.createAddFeatureIfHasFact(spirit_magic, spirit_magic_spells)
                                                        );
                progression.LevelEntries = entries.ToArray();
                progression.UIGroups = Helpers.CreateUIGroups(spirit_ability[0], greater_spirit_ability[0], true_spirit_ability[0], manifestation);


                wandering_progression = Helpers.CreateProgression(name + "WanderingSpiritProgression",
                                                                display_name + " Wandering Spirit",
                                                                description + $"\n{spells_description}",
                                                                "",
                                                                icon,
                                                                FeatureGroup.None,
                                                                Helpers.PrerequisiteNoFeature(progression),
                                                                Helpers.CreateAddFact(spirit_magic_spells));
                wandering_progression.LevelEntries = wandering_entries.ToArray();
                wandering_progression.UIGroups = Helpers.CreateUIGroups(spirit_ability[1], greater_spirit_ability[1], true_spirit_ability[1]);



                this.hex_selection = Helpers.CreateFeatureSelection(name + "SpiritHexSelection",
                                                               display_name + " Spirit Hex",
                                                               $"You can select one of the hexes granted by {display_name} spirit.",
                                                               "",
                                                               icon,
                                                               FeatureGroup.None,
                                                               Helpers.PrerequisiteFeature(progression)
                                                               );
                this.hex_selection.AllFeatures = hexes;
                this.hex_selection.HideInCharacterSheetAndLevelUp = true;


                wandering_hex_selection = Helpers.CreateFeatureSelection(name + "WanderingSpiritHexSelection",
                                                               display_name + " Spirit Hex",
                                                               $"You can select one of the hexes granted by {display_name} spirit.",
                                                               "",
                                                               icon,
                                                               FeatureGroup.None,
                                                               Helpers.PrerequisiteFeature(progression, any: true),
                                                               Helpers.PrerequisiteFeature(wandering_progression, any: true)
                                                               );
                wandering_hex_selection.AllFeatures = hexes;
                wandering_hex_selection.HideInCharacterSheetAndLevelUp = true;

            }
        }



        static BlueprintCharacterClass[] getShamanArray()
        {
            return new BlueprintCharacterClass[] { shaman_class };
        }


        internal static void createShamanClass()
        {
            Main.logger.Log("Shaman class test mode: " + test_mode.ToString());
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");

            shaman_class = Helpers.Create<BlueprintCharacterClass>();
            shaman_class.name = "ShamanClass";
            library.AddAsset(shaman_class, "6b1d00511e824f0fbc27ec8f54b8edb2");

            hex_engine = new HexEngine(getShamanArray(), StatType.Wisdom);

            shaman_class.LocalizedName = Helpers.CreateString("Shaman.Name", "Shaman");
            shaman_class.LocalizedDescription = Helpers.CreateString("Shaman.Description",
                "While some heroes speak to gods or consort with otherworldly muses, shamans commune with the spirits of the world and the energies that exist in every living thing. These divine adventurers draw upon their power to shape the world and expand the influence of their spiritual patrons. Shamans have strong ties to natural spirits. They form powerful bonds with particular spirits, and as their power grows they learn to call upon other spirits in times of need.\n"
                + "Role: Shamans make for potent divine spellcasters, capable of using divine spells and the power of their spirits to aid their allies and destroy their foes. While they aren’t the healers that clerics are, they can still fill that role when needed."
                );
            shaman_class.m_Icon = druid_class.Icon;
            shaman_class.SkillPoints = druid_class.SkillPoints;
            shaman_class.HitDie = DiceType.D8;
            shaman_class.BaseAttackBonus = druid_class.BaseAttackBonus;
            shaman_class.FortitudeSave = druid_class.ReflexSave;
            shaman_class.ReflexSave = druid_class.ReflexSave;
            shaman_class.WillSave = druid_class.WillSave;
            shaman_class.Spellbook = createShamanSpellbook();
            shaman_class.ClassSkills = new StatType[] {StatType.SkillKnowledgeArcana, StatType.SkillLoreNature, StatType.SkillLoreReligion, StatType.SkillPersuasion};
            shaman_class.IsDivineCaster = true;
            shaman_class.IsArcaneCaster = false;
            shaman_class.StartingGold = druid_class.StartingGold;
            shaman_class.PrimaryColor = druid_class.PrimaryColor;
            shaman_class.SecondaryColor = druid_class.SecondaryColor;
            shaman_class.RecommendedAttributes = new StatType[] { StatType.Wisdom, StatType.Charisma };
            shaman_class.NotRecommendedAttributes = new StatType[0];
            shaman_class.EquipmentEntities = druid_class.EquipmentEntities;
            shaman_class.MaleEquipmentEntities = druid_class.MaleEquipmentEntities;
            shaman_class.FemaleEquipmentEntities = druid_class.FemaleEquipmentEntities;
            shaman_class.ComponentsArray = new BlueprintComponent[] { druid_class.ComponentsArray[0] };
            shaman_class.StartingItems = new Kingmaker.Blueprints.Items.BlueprintItem[] {library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("511c97c1ea111444aa186b1a58496664"), //crossbow
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("ada85dae8d12eda4bbe6747bb8b5883c"), // quarterstaff
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("cd635d5720937b044a354dba17abad8d"), //s. cure light wounds
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("cd635d5720937b044a354dba17abad8d"), //s. cure light wounds
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("be452dba5acdd9441841d2189e1ae55a") //s.bless
                                                                                       };
            createShamanProgression();
            shaman_class.Progression = shaman_progression;
            createSpeakerForThePast();
            createWitchDoctor();
            createOverseer();
            shaman_class.Archetypes = new BlueprintArchetype[] {speaker_for_the_past_archetype, overseer_archetype, witch_doctor_archetype };
            Helpers.RegisterClass(shaman_class);
            createExtraHexFeat();

            Common.addMTDivineSpellbookProgression(shaman_class, shaman_class.Spellbook, "MysticTheurgeShaman",
                                                     Common.createPrerequisiteClassSpellLevel(shaman_class, 2));
        }


        static void createWitchDoctor()
        {
            createWitchDoctorChannel();
            createCounterCurse();

            witch_doctor_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "WitchDoctorArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Witch Doctor");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The witch doctor is a healer who specializes in afflictions of the soul. Often misunderstood, she protects her tribe with healing powers, powerful defensive magic, and her own divine “witchcraft.”");
            });
            Helpers.SetField(witch_doctor_archetype, "m_ParentClass", shaman_class);
            library.AddAsset(witch_doctor_archetype, "");
            witch_doctor_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(4, hex_selection),
                                                                              Helpers.LevelEntry(8, hex_selection),
                                                                              Helpers.LevelEntry(10, hex_selection),
                                                                              Helpers.LevelEntry(12, hex_selection),
                                                                             };

            witch_doctor_archetype.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(4, channel_energy),
                                                                   Helpers.LevelEntry(8, counter_curse)
                                                                  };

            shaman_progression.UIGroups = shaman_progression.UIGroups.AddToArray(Helpers.CreateUIGroups(channel_energy, counter_curse));
        }


        static void createWitchDoctorChannel()
        {
            var resource = Helpers.CreateAbilityResource("WitchDoctorChannelResource", "", "", "", null);
            resource.SetIncreasedByStat(3, StatType.Charisma);

            var positive_energy_feature = library.Get<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                                  type: AbilityRankType.Default, classes: getShamanArray(), startLevel: 4, stepLevel: 2);
            var dc_scaling = Common.createContextCalculateAbilityParamsBasedOnClasses(getShamanArray(), StatType.Charisma);
            channel_energy = Helpers.CreateFeature("ShamanWitchDoctorChannelPositiveEnergyFeature",
                                                   "Channel Positive Energy",
                                                   "At 4th level, the witch doctor can draw transcendental energies to her location, flooding it with positive energy as the cleric class feature. The witch doctor uses her shaman level – 3 as her effective cleric level, and can channel energy a number of times per day equal to 3 + her Charisma modifier. This is a separate pool of channel energy that does not stack with the life spirit’s channel spirit ability.",
                                                   "",
                                                   positive_energy_feature.Icon,
                                                   FeatureGroup.None);

            var heal_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHeal,
                                                                      "ShamanWitchDoctorChannelEnergyHealLiving",
                                                                      "",
                                                                      "Channeling positive energy causes a burst that heals all living creatures in a 30 - foot radius centered on the shaman. The amount of damage healed is equal to 1d6 plus 1d6 for every two shaman levels beyond fourth.",
                                                                      "",
                                                                      context_rank_config,
                                                                      dc_scaling,
                                                                      Helpers.CreateResourceLogic(resource));
            var harm_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHarm,
                                                          "ShamanWitchDoctorChannelEnergyHarmUndead",
                                                          "",
                                                          "Channeling energy causes a burst that damages all undead creatures in a 30 - foot radius centered on the shaman. The amount of damage dealt is equal to 1d6 plus 1d6 for every two shaman levels beyond fourth. Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1 / 2 the shaman's level + the shaman's Charisma modifier.",
                                                          "",
                                                          context_rank_config,
                                                          dc_scaling,
                                                          Helpers.CreateResourceLogic(resource));

            var heal_living_base = Common.createVariantWrapper("ShamanWitchDoctorPositiveHealBase", "", heal_living);
            var harm_undead_base = Common.createVariantWrapper("ShamanWitchDoctorPositiveHarmBase", "", harm_undead);

            ChannelEnergyEngine.storeChannel(heal_living, channel_energy, ChannelEnergyEngine.ChannelType.PositiveHeal);
            ChannelEnergyEngine.storeChannel(harm_undead, channel_energy, ChannelEnergyEngine.ChannelType.PositiveHarm);

            channel_energy.AddComponent(Helpers.CreateAddFacts(heal_living_base, harm_undead_base));
            channel_energy.AddComponent(Helpers.CreateAddAbilityResource(resource));
            var extra_channel = ChannelEnergyEngine.createExtraChannelFeat(heal_living, channel_energy, "ExtraChannelShamanWitchDoctor", "Extra Channel (Witch Doctor)", "");
        }


        static void createCounterCurse()
        {
            var remove_curse = library.Get<BlueprintAbility>("b48674cef2bff5e478a007cf57d8345b");
            var dispel_magic = library.Get<BlueprintAbility>("143775c49ae6b7446b805d3b2e702298");

            var dispels = new Common.SpellId[7];
            var removes = new Common.SpellId[7];

            var description = "At 8th level, the witch doctor can choose to lose any prepared spirit magic spell that is 3rd level or higher in order to spontaneously cast dispel magic or remove curse.This ability can only target a spell effect that is on an ally(including herself).If she forfeits a spirit magic spell higher than 3rd level, she gains a +2 sacred bonus on her caster level check to dispel the spell or to remove the curse for every spell level higher than 3rd that she sacrifices.";


            for (int i = 0; i < dispels.Length; i++)
            {
                var buff = Helpers.CreateBuff($"CounterCurse{i + 1}Buff",
                                              "",
                                              "",
                                              "",
                                              null,
                                              null,
                                              Helpers.Create<DispelCasterLevelCheckBonus>(d => d.Value = i * 2)
                                              );
                buff.SetBuffFlags(BuffFlags.HiddenInUi);
                var apply_buff =  Common.createContextActionOnContextCaster(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false));
                var remove_buff = Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(buff));
                var dispel = library.CopyAndAdd<BlueprintAbility>(dispel_magic, $"CounterCurseDispel{i + 1}Ability", "");
                dispel.SetName("Counter Curse: Dispel" + (i == 0 ? "" : $" (+{2 * i})"));
                dispel.SetDescription(description);
                dispel.Parent = null;
                dispel.CanTargetEnemies = false;
                dispel.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(new ContextAction[] { apply_buff }.AddToArray(a.Actions.Actions.AddToArray(remove_buff))));
                dispels[i] = new Common.SpellId(dispel.AssetGuid, 3 + i);

                var remove = library.CopyAndAdd<BlueprintAbility>(remove_curse, $"CounterCurseRemoveCurse{i + 1}Ability", "");
                remove.RemoveComponents<SpellListComponent>();
                remove.SetName("Counter Curse: Remove Curse" + (i == 0 ? "" : $" (+{2 * i})"));
                remove.SetDescription(description);
                remove.CanTargetEnemies = false;
                remove.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(new ContextAction[] { apply_buff }.AddToArray(a.Actions.Actions.AddToArray(remove_buff))));
                removes[i] = new Common.SpellId(remove.AssetGuid, 3 + i);
            }

            var dispel_spell_list = new Common.ExtraSpellList(dispels).createSpellList("CountercurseDispelSpellList", "");
            var removes_spell_list = new Common.ExtraSpellList(removes).createSpellList("CountercurseRemoveCurseSpellList", "");
            counter_curse = Helpers.CreateFeature("CounterCurseFeature",
                                                  "Counter Curse",
                                                  description,
                                                  "",
                                                  dispel_magic.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<NewMechanics.LearnSpellListToSpecifiedSpellbook>(l => {
                                                                                                                        l.spellbook = spirit_magic_spellbook;
                                                                                                                        l.SpellList = dispel_spell_list;
                                                                                                                       }
                                                                                                                  ),
                                                  Helpers.Create<NewMechanics.LearnSpellListToSpecifiedSpellbook>(l => {
                                                                                                                        l.spellbook = spirit_magic_spellbook;
                                                                                                                        l.SpellList = removes_spell_list;
                                                                                                                       }
                                                                                                                  )
                                                 );
        }


        static void createOverseer()
        {
            var spells = new Common.ExtraSpellList("88367310478c10b47903463c5d0152b0", //hypnotism
                                                   "fd4d9fd7f87575d47aafe2a64a6e2d8d", //hideous laughter
                                                   SpellDuplicates.addDuplicateSpell("c7104f7526c4c524f91474614054547e", "OverseerHoldPersonAbility").AssetGuid, //hold person
                                                   "4baf4109145de4345861fe0f2209d903", //crushing despair
                                                   "444eed6e26f773a40ab6e4d160c67faa", //feeblemind
                                                   "d316d3d94d20c674db2c24d7de96f6a7", //serenity
                                                   "261e1788bfc5ac1419eec68b1d485dbc", //power word blind
                                                   "f958ef62eea5050418fb92dfa944c631", //power word stun
                                                   "3c17035ec4717674cae2e841a190e757"//domiante monster
                                                   );
            controlling_magic_spell_list = spells.createSpellList("ControllingMagicOverseerSpelllist", "");
            controlling_magic = Helpers.CreateFeature("ControllingMagicFeature",
                                                      "Controlling Magic",
                                                      "The overseer adds the following spells to the list of spells she can cast using spirit magic: hypnotism (1st), hideous laughter (2nd), hold person (3rd), crushing despair (4th), feeblemind (5th), serenity (6th), power word blind (7th), power word stun (8th), and dominate monster (9th). This ability replaces the spirit magic spells gained from the shaman’s spirit.",
                                                      "",
                                                      null,
                                                      FeatureGroup.None,
                                                      Helpers.Create<NewMechanics.LearnSpellListToSpecifiedSpellbook>(l => {
                                                                                                                      l.spellbook = spirit_magic_spellbook;
                                                                                                                      l.SpellList = controlling_magic_spell_list;
                                                                                                                     })
                                                     );
            var feature = Helpers.CreateFeature("ControllingMagicFluidMagicFeature",
                                    "",
                                    "",
                                    "",
                                    null,
                                    FeatureGroup.None,
                                    Helpers.Create<LearnSpellList>(l => { l.CharacterClass = shaman_class; l.SpellList = controlling_magic_spell_list; })
                                    );
            feature.HideInCharacterSheetAndLevelUp = true;
            WavesSpirit.fluid_magic.AddComponent(Common.createAddFeatureIfHasFact(controlling_magic, feature));

            overseer_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "OverseerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Overseer");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While all shamans use their connection to the spirits of the world to draw upon otherworldly magic powers, some shamans have a tradition in which they use the power of patron spirits to directly control their enemies. Such overseers may assume roles as religious leaders and protectors of their tribes, turning foes into short-term allies for the tribe’s greater good. Other overseers become tyrants who enforce their will upon the weak for personal gain. In combat, an overseer manages the battlefield by debilitating foes using her hexes and specialized spells.");
            });
            Helpers.SetField(overseer_archetype, "m_ParentClass", shaman_class);
            library.AddAsset(overseer_archetype, "");
            overseer_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, spirit_magic)};

            overseer_archetype.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, controlling_magic)};
        }

        
        static void createSpeakerForThePast()
        {
            mystery_engine = new MysteryEngine(getShamanArray(), StatType.Wisdom);

            var aging_touch = mystery_engine.createAgingTouch("AgingTouchShamanRevelation",
                                                              "Aging Touch",
                                                              "Your touch ages living creatures and objects. As a melee touch attack, you can deal 1 point of Strength damage for every two shaman levels you possess to living creatures. Against constructs, you can deal 1d6 points of damage per shaman level. You can use this ability once per day, plus one additional time per day for every five shaman levels you possess.");
            var blood_of_heroes = mystery_engine.createBloodOfHeroes("BloodOfHeroesShamanRevelation",
                                                                     "Blood of Heroes",
                                                                     "As a move action, you can call upon your ancestors to grant you extra bravery in battle. You gain a +1 morale bonus on attack rolls, damage rolls, and Will saves against fear for a number of rounds equal to your Wisdom bonus. At 7th level, this bonus increases to +2, and at 14th level this bonus increases to +3. You can use this ability once per day, plus one additional time per day at 5th level, and every five levels thereafter.");
            var phantom_touch = mystery_engine.createPhantomTouch("PhantomTouchShamanRevelation",
                                                                  "Phantom Touch",
                                                                  "As a standard action, you can perform a melee touch attack that causes a living creature to become shaken. This ability lasts for a number of rounds equal to 1/2 your shaman level (minimum 1 round). You can use this ability a number of times per day equal to 3 + your Wisdom modifier.");
            var sacred_council = mystery_engine.createSacredCouncil("SacredCouncilShamanRevelation",
                                                                    "Sacred Council",
                                                                     "As a move action, you can call upon your ancestors to provide council. This advice grants you a +2 bonus on any one kind of d20 rolls. This effect lasts for 1 round. You can use this ability a number of times per day equal to your Wisdom bonus.");
            var spirit_of_the_warrior = mystery_engine.createSpiritOfTheWarrior("SpiritOfTheWarriorShamanRevelation",
                                                                                "Spirit of the Warrior",
                                                                                "You can summon the spirit of a great warrior ancestor and allow it to possess you, becoming a mighty warrior yourself. You gain a +4 enhancement bonus to Strength, Dexterity, and Constitution, and a +4 natural armor bonus to AC. Your base attack bonus while possessed equals your shaman level (which may give you additional attacks), all weapons you are holding receive keen enchantment. You can use this ability for 1 round for every 2 shaman levels you possess. This duration does not need to be consecutive, but it must be spent in 1-round increments. You must be at least 11th level to select this revelation.");
            var speed_or_slow_time = mystery_engine.createSpeedOrSlowTime("SpeedOrSlowTimeShamanRevelation",
                                                                          "Speed or Slow Time",
                                                                          "As a standard action, you can speed up or slow down time, as either the haste or slow spell. You can use this ability once per day, plus one additional time per day at 12th level and 17th level. You must be at least 7th level before selecting this revelation.");
            var spirit_shield = hex_engine.createAirBarrier("SpiritShieldShamanRevelation",
                                                            "Spirit Shield",
                                                            "You can call upon the spirits of your ancestors to form a shield around you that blocks incoming attacks and grants you a +4 armor bonus. At 7th level, and every four levels thereafter, this bonus increases by +2. At 13th level, this shield causes arrows, rays, and other ranged attacks requiring an attack roll against you to have a 50% miss chance. You can use this shield for 1 hour per day per shaman level. This duration does not need to be consecutive, but it must be spent in 1-hour increments.");
            var storm_of_souls = mystery_engine.createStormOfSouls("StormOfSoulsShamanRevelation",
                                                                   "Storm of Souls",
                                                                   "You can summon the spirits of your ancestors to attack in a ghostly barrage—their fury creates physical wounds on creatures in the area. The storm has a range of 100 feet and is a 20-foot-radius burst. Objects and creatures in the area take 1d8 hit points of damage for every two shaman levels you possess. Undead creatures in the area take 1d8 points of damage for every shaman level you possess. A successful Fortitude save reduces the damage to half. You must be at least 7th level to select this revelation. You can use this ability once per day, plus one additional time per day at 11th level and every four levels thereafter.");
            var temporal_celerity = mystery_engine.createTemporalCelerity("TemporalCelerityShamanRevelation",
                                                                      "Temporal Celerity",
                                                                      "Whenever you roll for initiative, you can roll twice and take either result. At 7th level, you can always act in the surprise round, but if you fail to notice the ambush, you act last, regardless of your initiative result (you act in the normal order in following rounds). At 11th level, you can roll for initiative three times and take any one of the results.");
            var time_flicker = mystery_engine.createTimeFlicker("TimeFlickerShamanRevelation",
                                                                "Time Flicker",
                                                                "As a standard action, you can flicker in and out of time, gaining concealment (as the blur spell). You can use this ability for 1 minute per shaman level that you possess per day. This duration does not need to be consecutive, but it must be spent in 1-minute increments. At 7th level, each time you activate this ability, you can treat it as the displacement spell, though each round spent this way counts as 1 minute of your normal time flicker duration. You must be at least 3rd level to select this revelation.");
            var time_hop = mystery_engine.createTimeHop("TimeHopShamanRevelation",
                                                        "Time Hop",
                                                        "As a move action, you can teleport up to 50 feet per 3 shaman levels, as the dimension door spell. This movement does not provoke attacks of opportunity. You must have line of sight to your destination to use this ability. You can bring other willing creatures with you, but you must expend 2 uses of this ability. You must be at least 7th level before selecting this revelation.");
            var time_sight = mystery_engine.createTimeSight("TimeSightShamanRevelation",
                                                        "Time Sight",
                                                        "You can peer through the mists of time to see things as they truly are, as if using the true seeing spell.\n" +
                                                        "At 18th level, this functions like foresight. You can use this ability for a number of minutes per day equal to your oracle level, but these minutes do not need to be consecutive. You must be at least 11th level to select this revelation.");

            revelation_selection = Helpers.CreateFeatureSelection("RevelationPastSelection",
                                                                      "Revelations of the Past",
                                                                      "At 4th, 6th, 12th, 14th, and 20th levels, the speaker for the past can select a revelation from the ancestor or time mysteries. She uses her shaman level as her oracle level for these revelations, and uses her Wisdom modifier in place of her Charisma modifier for the purposes of the revelation.",
                                                                      "",
                                                                      null,
                                                                      FeatureGroup.None);
            revelation_selection.AllFeatures = new BlueprintFeature[] {aging_touch, blood_of_heroes, phantom_touch, sacred_council, spirit_of_the_warrior, speed_or_slow_time,
                                                                       spirit_shield, storm_of_souls, temporal_celerity, time_flicker, time_hop, time_sight};

            var spells = new Common.ExtraSpellList("2c38da66e5a599347ac95b3294acbe00", //true strike
                                               NewSpells.force_sword.AssetGuid, //force sword,
                                               "5ab0d42fb68c9e34abae4921822b9d63", //heroism
                                               "6717dbaef00c0eb4897a1c908a75dfe5", //phantasmal killer
                                               "90810e5cf53bf854293cbd5ea1066252", //righteous might
                                               NewSpells.contingency.AssetGuid,
                                               "4aa7942c3e62a164387a73184bca3fc1", //disintegrate
                                               "0e67fa8f011662c43934d486acc50253", //predicition of failure
                                               "43740dab07286fe4aa00a6ee104ce7c1" //heroic invocation
                                               );
            mysteries_of_the_past = Helpers.CreateFeature("MysteriesOfThePastSpeakerForThePastFeature",
                                                          "Mysteries of the past",
                                                          "A speaker for the past gains Knowledge World, Perception, and Use Magic Device as class skills. She also adds the following spells to her spell list: True Strike (1st), Force Sword (2nd), Heroism (3rd), Phantasmal Killer (4th), Righteous Might (5th), Contingency (6th), Disintegrate (7th), Prediction of Failure (8th), Heroic Invocation (9th).",
                                                          "",
                                                          null,
                                                          FeatureGroup.None,
                                                          spells.createLearnSpellList("MysteriesOfThePastSpellList", "", shaman_class)
                                                          );
            speaker_for_the_past_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SpeakerForThePastArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Speaker For The Past");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A speaker for the past is a shaman who specifically serves as the voice for spirits from her people’s history. A speaker for the past is often an advocate of the ancestors of a specific group, the voice of experience, and a powerful resource that allows the past to aid the present.");
            });
            Helpers.SetField(speaker_for_the_past_archetype, "m_ParentClass", shaman_class);
            library.AddAsset(speaker_for_the_past_archetype, "");
            speaker_for_the_past_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, shaman_familiar),
                                                                              Helpers.LevelEntry(4, wandering_shaman_spirits),
                                                                              Helpers.LevelEntry(6, wandering_hex_selection),
                                                                              Helpers.LevelEntry(14, wandering_hex_selection),
                                                                             };

            speaker_for_the_past_archetype.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, mysteries_of_the_past),
                                                                           Helpers.LevelEntry(4, revelation_selection),
                                                                           Helpers.LevelEntry(6, revelation_selection),
                                                                           Helpers.LevelEntry(12, revelation_selection),
                                                                           Helpers.LevelEntry(14, revelation_selection),
                                                                           Helpers.LevelEntry(20, revelation_selection),
                                                                         };

            shaman_progression.UIGroups = shaman_progression.UIGroups.AddToArray(Helpers.CreateUIGroups(mysteries_of_the_past, revelation_selection, revelation_selection, revelation_selection, revelation_selection, revelation_selection));

            speaker_for_the_past_archetype.ReplaceClassSkills = true;
            speaker_for_the_past_archetype.ClassSkills = shaman_class.ClassSkills.AddToArray(StatType.SkillKnowledgeWorld, StatType.SkillPerception, StatType.SkillUseMagicDevice);
        }


        static void createExtraHexFeat()
        {
            extra_hex = library.CopyAndAdd<BlueprintFeature>(hex_selection.AssetGuid, "ShamanExtraHexFeat", "");
            extra_hex.SetNameDescriptionIcon("Extra Shaman Hex", 
                                             "You gain one additional hex. It must be a general hex or a hex granted by your spirit rather than one from a wandering spirit.",
                                             Witch.extra_hex_feat.Icon);
            extra_hex.AddComponent(Helpers.PrerequisiteClassLevel(shaman_class, 2));
            extra_hex.Groups = new FeatureGroup[] { FeatureGroup.Feat };
            library.AddFeats(extra_hex);
        }


        static BlueprintSpellbook createShamanSpellbook()
        {
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var shaman_spellbook = Helpers.Create<BlueprintSpellbook>();
            shaman_spellbook.name = "ShamanSpellbook";
            library.AddAsset(shaman_spellbook, "20fbd5cd3f79455aa9f133ecf21797ab");
            shaman_spellbook.Name = shaman_class.LocalizedName;
            shaman_spellbook.SpellsPerDay = druid_class.Spellbook.SpellsPerDay;
            shaman_spellbook.SpellsKnown = druid_class.Spellbook.SpellsKnown;
            shaman_spellbook.Spontaneous = false;
            shaman_spellbook.IsArcane = false;
            shaman_spellbook.AllSpellsKnown = true;
            shaman_spellbook.CanCopyScrolls = false;
            shaman_spellbook.CastingAttribute = StatType.Wisdom;
            shaman_spellbook.CharacterClass = shaman_class;
            shaman_spellbook.CasterLevelModifier = 0;
            shaman_spellbook.CantripsType = CantripsType.Orisions;
            shaman_spellbook.SpellsPerLevel = druid_class.Spellbook.SpellsPerLevel;

            shaman_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            shaman_spellbook.SpellList.name = "ShamanSpellList";
            library.AddAsset(shaman_spellbook.SpellList, "7113337f695742559ecdecc8905b132a");
            shaman_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < shaman_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                shaman_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);

            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "55f14bc84d7c85446b07a1b5dd6b2b4c", 0), //daze
                new Common.SpellId( "c3a8f31778c3980498d8f00c980be5f5", 0), //guidance
                new Common.SpellId( "95f206566c5261c42aa5b3e7e0d1e36c", 0), //mage light
                new Common.SpellId( "7bc8e27cba24f0e43ae64ed201ad5785", 0), //resistance
                new Common.SpellId( "5bf3315ce1ed4d94e8805706820ef64d", 0), //touch of fatigue
                new Common.SpellId( "d3a852385ba4cd740992d1970170301a", 0), //virtue

                new Common.SpellId( "8bc64d869456b004b9db255cdd1ea734", 1), //bane
                new Common.SpellId( "90e59f4a4ada87243b7b3535a06d0638", 1), //bless
                new Common.SpellId( "4783c3709a74a794dbe7c8e7e0b1b038", 1), //burning hands
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( NewSpells.chill_touch.AssetGuid, 1),
                new Common.SpellId( "5590652e1c2225c4ca30c4a699ab3649", 1), //cure light wounds
                new Common.SpellId( "fbdd8c455ac4cde4a9a3e18c84af9485", 1), //doom
                new Common.SpellId( "0fd00984a2c0e0a429cf1a911b4ec5ca", 1), //entangle
                new Common.SpellId( NewSpells.frost_bite.AssetGuid, 1),
                new Common.SpellId( "40ec382849b60504d88946df46a10f2d", 1), //haze of dreams
                new Common.SpellId( HexEngine.hex_vulnerability_spell.AssetGuid, 1), //hex vilnerability
                new Common.SpellId( "e5af3674bb241f14b9a9f6b0c7dc3d27", 1), //inflict light wounds
                new Common.SpellId( NewSpells.obscuring_mist.AssetGuid, 1),
                new Common.SpellId( NewSpells.produce_flame.AssetGuid, 1),
                new Common.SpellId( "433b1faf4d02cc34abb0ade5ceda47c4", 1), //protection from alignment
                new Common.SpellId( "55a037e514c0ee14a8e3ed14b47061de", 1), //remove fear
                new Common.SpellId( "bb7ecad2d3d2c8247a38f44855c99061", 1), //sleep
                new Common.SpellId( "c6147854641924442a3bb736080cfeb6", 1), //summon nature ally I
                new Common.SpellId( "dd38f33c56ad00a4da386c1afaa49967", 1), //ubreakable heart
                

                new Common.SpellId( "03a9630394d10164a9410882d31572f0", 2), //aid
                new Common.SpellId( "5b77d7cc65b8ab74688e74a37fc2f553", 2), //barksin
                new Common.SpellId( "a900628aea19aa74aad0ece0e65d091a", 2), //bear's endurance
                new Common.SpellId( NewSpells.bone_fists.AssetGuid, 2),
                new Common.SpellId( "4c3d08935262b6544ae97599b3a9556d", 2), //bull's strength
                new Common.SpellId( "6b90c773a6543dc49b2505858ce33db5", 2), //cure moderate wounds
                new Common.SpellId( "b48b4c5ffb4eab0469feba27fc86a023", 2), //delay poison
                new Common.SpellId( "446f7bf201dc1934f96ac0a26e324803", 2), //eagle's splendor
                new Common.SpellId( "7a5b5bf845779a941a67251539545762", 2), //false life
                new Common.SpellId( NewSpells.flame_blade.AssetGuid, 2),
                new Common.SpellId( "c7104f7526c4c524f91474614054547e", 2), //hold person
                new Common.SpellId( "65f0b63c45ea82a4f8b8325768a3832d", 2), //inflict moderate wounds
                new Common.SpellId( "f0455c9295b53904f9e02fc571dd2ce1", 2), //owl's wisdom
                new Common.SpellId( "f8bce986adfc88544a42bf4ab7ae75b2", 2), //remove paralysis
                new Common.SpellId( "21ffef7791ce73f468b6fca4d9371e8b", 2), //resist energy
                new Common.SpellId( "e84fc922ccf952943b5240293669b171", 2), //restoration lesser
                new Common.SpellId( "08cb5f4c3b2695e44971bf5c45205df0", 2), //scare
                new Common.SpellId( "6c7467f0344004d48848a43d8c078bf8", 2), //sickening entanglement
                new Common.SpellId( "298148133cdc3fd42889b99c82711986", 2), //summon nature ally II
                new Common.SpellId( NewSpells.vine_strike.AssetGuid, 2),
                new Common.SpellId( NewSpells.winter_grasp.AssetGuid, 2),

                new Common.SpellId( "4b76d32feb089ad4499c3a1ce8e1ac27", 3), //animate dead
                new Common.SpellId( "989ab5c44240907489aba0a8568d0603", 3), //bestow curse
                new Common.SpellId( "46fd02ad56c35224c9c91c88cd457791", 3), //blindness
                new Common.SpellId( "2a9ef0e0b5822a24d88b16673a267456", 3), //call lightning
                new Common.SpellId( "3361c5df793b4c8448756146a88026ad", 3), //cure serious wounds
                new Common.SpellId( "7658b74f626c56a49939d9c20580885e", 3), //deep slumber
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "754c478a2aa9bb54d809e648c3f7ac0e", 3), //dominate animal
                new Common.SpellId( NewSpells.earth_tremor.AssetGuid, 3),
                new Common.SpellId( "bd5da98859cf2b3418f6d68ea66cabbe", 3), //inflict serious wounds
                new Common.SpellId( "2d4263d80f5136b4296d6eb43a221d7d", 3), //magic vestment
                new Common.SpellId( "c927a8b0cd3f5174f8c0b67cdbfde539", 3), //remove blindness
                new Common.SpellId( "b48674cef2bff5e478a007cf57d8345b", 3), //remove curse
                new Common.SpellId( "4093d5a0eb5cae94e909eb1e0e1a6b36", 3), //remove disease
                new Common.SpellId( "1a36c8b9ed655c249a9f9e8d4731f001", 3), //soothing mud
                new Common.SpellId( "68a9e6d7256f1354289a39003a46d826", 3), //stinking cloud
                new Common.SpellId( "fdcf7e57ec44f704591f11b45f4acf61", 3), //summon nature ally III

                new Common.SpellId( "41c9016596fe1de4faf67425ed691203", 4), //cure critical wounds
                new Common.SpellId( "ef16771cb05d1344989519e87f25b3c5", 4), //divine power
                new Common.SpellId( "dc6af3b4fd149f841912d8a3ce0983de", 4), //false life, greater
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( "fcb028205a71ee64d98175ff39a0abf9", 4), //ice storm
                new Common.SpellId( "651110ed4f117a948b41c05c5c7624c0", 4), //inflict critical wounds
                new Common.SpellId( "a8666d26bbbd9b640958284e0eee3602", 4), //life blast
                new Common.SpellId( "e7240516af4241b42b2cd819929ea9da", 4), //neutralize poison
                new Common.SpellId( "d797007a142a6c0409a74b064065a15e", 4), //poison
                new Common.SpellId( "f2115ac1148256b4ba20788f7e966830", 4), //restoration
                new Common.SpellId( "6b30813c3709fc44b92dc8fd8191f345", 4), //slowing mud
                new Common.SpellId( "d1afa8bc28c99104da7d784115552de5", 4), //spike stones
                new Common.SpellId( "9779c8578acd919419f563c33d7b2af5", 4), //spit venom           
                new Common.SpellId( "c83db50513abdf74ca103651931fac4b", 4), //summon nature ally IV
                new Common.SpellId( "2daf9c5112f16d54ab3cd6904c705c59", 4), //thorn body

                new Common.SpellId( "56923211d2ac95e43b8ac5031bab74d8", 5), //animal growth
                new Common.SpellId( "3105d6e9febdc3f41a08d2b7dda1fe74", 5), //baleful polymorph
                new Common.SpellId( "7792da00c85b9e042a0fdfc2b66ec9a8", 5), //break enchantment
                new Common.SpellId( "d5847cad0b0e54c4d82d6c59a3cda6b0", 5), //breath of life
                new Common.SpellId( "d5a36a7ee8177be4f848b953d1c53c84", 5), //call lightning storm
                new Common.SpellId( "bacba2ff48d498b46b86384053945e83", 5), //cave fangs
                new Common.SpellId( "5d3d689392e4ff740a761ef346815074", 5), //cure light wounds mass
                new Common.SpellId( "d7cbd2004ce66a042aeab2e95a3c5c61", 5), //dominate person
                new Common.SpellId( "f9910c76efc34af41b6e43d5d8752f0f", 5), //flamestrike
                new Common.SpellId( "9da37873d79ef0a468f969e4e5116ad2", 5), //inflict light wounds mass
                new Common.SpellId( "c66e86905f7606c4eaa5c774f0357b2b", 5), //stoneskin
                new Common.SpellId( "8f98a22f35ca6684a983363d32e51bfe", 5), //summon nature ally
                new Common.SpellId( "b3da3fbee6a751d4197e446c7e852bcb", 5), //true seeing

                new Common.SpellId( "d361391f645db984bbf58907711a146a", 6), //banishment
                new Common.SpellId( "f6bcea6db14f0814d99b54856e918b92", 6), //bears endurance mass
                new Common.SpellId( "6a234c6dcde7ae94e94e9c36fd1163a7", 6), //bulls strength mass
                new Common.SpellId( "e7c530f8137630f4d9d7ee1aa7b1edc0", 6), //cone of cold
                new Common.SpellId( "571221cc141bc21449ae96b3944652aa", 6), //cure moderate wounds mass              
                new Common.SpellId( "f0f761b808dc4b149b08eaf44b99f633", 6), //dispel magic greater
                new Common.SpellId( "2caa607eadda4ab44934c5c9875e01bc", 6), //eagle's splendor mass
                new Common.SpellId( "03944622fbe04824684ec29ff2cec6a7", 6), //inflict moderate wounds mass
                new Common.SpellId( "9f5ada581af3db4419b54db77f44e430", 6), //owl's wisdom mass
                new Common.SpellId( "a0fc99f0933d01643b2b8fe570caa4c5", 6), //raise dead
                new Common.SpellId( "a6e59e74cba46a44093babf6aec250fc", 6), //slay living
                new Common.SpellId( "051b979e7d7f8ec41b9fa35d04746b33", 6), //summon nature ally VI
                new Common.SpellId( "e243740dfdb17a246b116b334ed0b165", 6), //stone to flesh
                
                new Common.SpellId( "7f71a70d822af94458dc1a235507e972", 7), //cloak of dreams
                new Common.SpellId( "b974af13e45639a41a04843ce1c9aa12", 7), //creeping doom
                new Common.SpellId( "0cea35de4d553cc439ae80b3a8724397", 7), //cure serious wounds mass
                new Common.SpellId( "137af566f68fd9b428e2e12da43c1482", 7), //harm
                new Common.SpellId( "ff8f1534f66559c478448723e16b6624", 7), //heal              
                new Common.SpellId( "820170444d4d2a14abc480fcbdb49535", 7), //inflict serious wounds mass
                new Common.SpellId( "da1b292d91ba37948893cdbe9ea89e28", 7), //legendary proportions
                new Common.SpellId( "76a11b460be25e44ca85904d6806e5a3", 7), //create undead
                new Common.SpellId( "fafd77c6bfa85c04ba31fdc1c962c914", 7), //restoration greater
                new Common.SpellId( "051b979e7d7f8ec41b9fa35d04746b33", 7), //summon nature ally VII
                new Common.SpellId( "1fca0ba2fdfe2994a8c8bc1f0f2fc5b1", 7), //sunbeam
                new Common.SpellId( "474ed0aa656cc38499cc9a073d113716", 7), //umbral strike
                
                new Common.SpellId(NewSpells.blood_mist.AssetGuid, 8),
                new Common.SpellId( "1f173a16120359e41a20fc75bb53d449", 8), //cure critical wounds mass
                new Common.SpellId( "3b646e1db3403b940bf620e01d2ce0c7", 8), //destruction
                new Common.SpellId( "e3d0dfe1c8527934294f241e0ae96a8d", 8), //firestorm
                new Common.SpellId( "08323922485f7e246acb3d2276515526", 8), //horrid wilting
                new Common.SpellId( "5ee395a2423808c4baf342a4f8395b19", 8), //inflict critical wounds mass
                new Common.SpellId( "80a1a388ee938aa4e90d427ce9a7a3e9", 8), //ressurection
                new Common.SpellId( "7cfbefe0931257344b2cb7ddc4cdff6f", 8), //stormbolts
                new Common.SpellId( "ea78c04f0bd13d049a1cce5daf8d83e0", 8), //summon nature ally
                new Common.SpellId( "e96424f70ff884947b06f41a765b7658", 8), //sunburst

                
                new Common.SpellId( "0340fe43f35e7a448981b646c638c83d", 9), //elemental swarm
                new Common.SpellId( "37302f72b06ced1408bf5bb965766d46", 9), //energy drain
                new Common.SpellId( "1f01a098d737ec6419aedc4e7ad61fdd", 9), //foresight
                new Common.SpellId( "867524328b54f25488d371214eea0d90", 9), //heal mass
                new Common.SpellId( "ba48abb52b142164eba309fd09898856", 9), //polar midnight
                new Common.SpellId(Wildshape.shapechange.AssetGuid, 9),
                new Common.SpellId( "d8144161e352ca846a73cf90e85bf9ac", 9), //tsunami
                new Common.SpellId( "a7469ef84ba50ac4cbf3d145e3173f8e", 9), //summon nature ally IX
                new Common.SpellId( "b24583190f36a8442b212e45226c54fc", 9), //wail of banshee
                new Common.SpellId(NewSpells.winds_of_vengeance.AssetGuid, 9)
            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(shaman_spellbook.SpellList, spell_id.level);
            }

            return shaman_spellbook;
        }


        static void createShamanProgression()
        {
            createShamanOrisons();
            createShamanFamiliar();
            createShamanProficiencies();
            createSpiritMagic();
            createSpirits();
            createHexes();
            createHexSelections();
            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            shaman_progression = Helpers.CreateProgression("ShamanProgression",
                                                               shaman_class.Name,
                                                               shaman_class.Description,
                                                               "",
                                                               shaman_class.Icon,
                                                               FeatureGroup.None);
            shaman_progression.Classes = getShamanArray();

            shaman_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, shaman_proficiencies, detect_magic, shaman_orisons,
                                                                                        spirit_magic,
                                                                                        shaman_spirits,
                                                                                        shaman_familiar,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),  // touch calculate feature};
                                                                    Helpers.LevelEntry(2, hex_selection),
                                                                    Helpers.LevelEntry(3),
                                                                    Helpers.LevelEntry(4, hex_selection, wandering_shaman_spirits),
                                                                    Helpers.LevelEntry(5),
                                                                    Helpers.LevelEntry(6, wandering_hex_selection),
                                                                    Helpers.LevelEntry(7),
                                                                    Helpers.LevelEntry(8, hex_selection),
                                                                    Helpers.LevelEntry(9),
                                                                    Helpers.LevelEntry(10, hex_selection),
                                                                    Helpers.LevelEntry(11),
                                                                    Helpers.LevelEntry(12, hex_selection),
                                                                    Helpers.LevelEntry(13),
                                                                    Helpers.LevelEntry(14, wandering_hex_selection),
                                                                    Helpers.LevelEntry(15),
                                                                    Helpers.LevelEntry(16, hex_selection),
                                                                    Helpers.LevelEntry(17),
                                                                    Helpers.LevelEntry(18, hex_selection),
                                                                    Helpers.LevelEntry(19),
                                                                    Helpers.LevelEntry(20, hex_selection)
                                                                    };

            shaman_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] {shaman_proficiencies, detect_magic, shaman_orisons, shaman_spirits, shaman_familiar};

            shaman_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(hex_selection, hex_selection, hex_selection, hex_selection, hex_selection, hex_selection, hex_selection, hex_selection),
                                                         Helpers.CreateUIGroup(wandering_shaman_spirits, wandering_hex_selection, wandering_hex_selection)
                                                        };
        }


        static void createSpirits()
        {
            spirits.Add("Battle", BattleSpirit.create());
            spirits.Add("Bones", BonesSpirit.create());
            spirits.Add("Flame", FlameSpirit.create());
            spirits.Add("Stone", StoneSpirit.create());
            spirits.Add("Waves", WavesSpirit.create());
            spirits.Add("Wind", WindSpirit.create());
            spirits.Add("Nature", NatureSpirit.create());
            spirits.Add("Life", LifeSpirit.create());
            spirits.Add("Lore", LoreSpirit.create());


            foreach (var s in spirits)
            {
                var feature = Helpers.CreateFeature(s.Key + "FluidMagicFeature",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    FeatureGroup.None,
                                                    Helpers.Create<LearnSpellList>(l => { l.CharacterClass = shaman_class; l.SpellList = s.Value.spirit_magic_spell_list; })
                                                    );
                feature.HideInCharacterSheetAndLevelUp = true;

                WavesSpirit.fluid_magic.AddComponent(Common.createAddFeatureIfHasFact(s.Value.spirit_magic_spells, feature));
            }


            shaman_spirits = Helpers.CreateFeatureSelection("ShamanSpiritSelection",
                                                           "Spirit",
                                                           "A shaman forms a mystical bond with the spirits of the world. She forms a lasting bond with a single spirit, which grants a number of abilities and defines many of her other class features.\n"
                                                           + "At 1st level, a shaman gains the spirit ability granted by her chosen spirit. She adds the spells granted by that spirit to the list of spells that she can cast using spirit magic. She also adds the hexes possessed by that spirit to the list of hexes that she can use with the hex and wandering hex class features.\n"
                                                           + "At 8th level, the shaman gains the abilities listed in the greater version of her selected spirit. At 16th level, the shaman gains the abilities listed for the true version of her selected spirit.",
                                                           "",
                                                           null,
                                                           FeatureGroup.None);

            wandering_shaman_spirits = Helpers.CreateFeatureSelection("WanderingShamanSpiritSelection",
                                                                       "Wandering Spirit",
                                                                       "At 4th level, a shaman can form a bond with a spirit other than the one selected using her spirit class feature. She gains the spirit ability granted by the spirit. She also adds the spells granted by that spirit to her list of spells that she can cast using spirit magic. She does not add the hexes from her wandering spirit to her list of hexes that she can choose from with the hex class feature. At 12th level, she gains the abilities listed in the greater version of her wandering spirit. At 20th level, she gains the ability listed in the true version of her wandering spirit.",
                                                                       "",
                                                                       null,
                                                                       FeatureGroup.None);

            foreach (var s in spirits)
            {
                shaman_spirits.AllFeatures = shaman_spirits.AllFeatures.AddToArray(s.Value.progression);
                wandering_shaman_spirits.AllFeatures = wandering_shaman_spirits.AllFeatures.AddToArray(s.Value.wandering_progression);
            }
        }


        static void createHexes()
        {
            healing = hex_engine.createHealing("ShamanHealing", "Healing",
                                               " A shaman soothes the wounds of those she touches. This acts as cure light wounds, using the shaman’s caster level. Once a creature has benefited from the healing hex, it cannot benefit from it again for 24 hours. At 5th level, this acts as cure moderate wounds.",
                                               "", "", "", "", "", "");
            misfortune_hex = hex_engine.createMisfortune("ShamanMisfortune",
                                                         "Misfortune",
                                                         "The shaman causes a creature within 30 feet to suffer grave misfortune for 1 round. Anytime the creature makes an ability check, attack roll, saving throw, or skill check, it must roll twice and take the worse result. A successful Will saving throw negates this hex. At 8th level and 16th level, the duration of this hex is extended by 1 round. This hex affects all rolls the target must make while it lasts. Whether or not the save is successful, the creature cannot be the target of this hex again for 24 hours.",
                                                         "", "", "", "");
            fortune_hex = hex_engine.createFortuneHex("ShamanFortune",
                                                      "Fortune",
                                                      "The shaman grants a creature within 30 feet a bit of good luck for 1 round. The target can call upon this good luck, allowing it to reroll any ability check, attack roll, saving throw, or skill check, taking the better result. The target creature must to decide to use this ability before the first roll is made. At 8th and 16th levels, the duration of this hex increases by 1 round. Once a creature has benefited from the fortune hex, it cannot benefit from it again for 24 hours.",
                                                      "", "", "", "");
            evil_eye = hex_engine.createEvilEye("ShamanEvilEye",
                                                "Evil Eye",
                                                "The shaman causes doubt to creep into the mind of a foe within 30 feet that she can see. The target takes a –2 penalty on one of the following (shaman’s choice): ability checks, AC, attack rolls, saving throws, or skill checks. This hex lasts a number of rounds equal to 3 + the shaman’s Wisdom modifier. A successful Will saving throw reduces this to just 1 round. At 8th level, the penalty increases to –4. This is a mind-affecting effect.",
                                                "", "", "", "", "", "", "", "");
            ward = hex_engine.createWardHex("ShamanWard",
                                            "Ward",
                                            "The shaman places a protective ward over one creature. The warded creature receives a +2 deflection bonus to AC and a +2 resistance bonus on saving throws. This effect lasts until the warded creature is hit or fails a saving throw. A shaman knows when a warded creature is no longer protected. A shaman can have only one ward active at a time. If the shaman uses this hex while a previous ward is still active, that previous ward immediately ends. A shaman cannot use this ability on herself. At 8th and 16th levels, the bonuses provided by this ward increase by 1.");
            shapeshift = hex_engine.createShapeshiftHex("ShamanShapeshift",
                                                        "Shapeshift",
                                                        "The shaman transforms herself into another form for a number of minutes per day equal to her level, as alter self. This duration does not need to be consecutive, but must be spent in 1-minute increments. Changing form (including changing back) is a standard action that doesn’t provoke an attack of opportunity. At 8th level, this ability works as beast shape I. At 12th level, this ability works as beast shape II. At 16th level, this ability works as beast shape III. At 20th level, this ability works as beast shape IV.");
            wings_attack_hex = hex_engine.CreateWingsAttackHex("ShamanWingsAttack",
                                                               "Wings I",
                                                               "The shaman can grow a pair of weak wings. Initially, these wings lack the power to allow the shaman to fly, but the shaman can use them as a secondary natural attack that deals 1d4 points of damage (1d3 for a Small shaman).\n"
                                                               + "A shaman of 8th level or higher can select the wings hex a second time. This allows her to ignore ground-based effects and gives +3 dodge bonus to AC against melee attacks.");
            wings_hex = hex_engine.CreateWingsHex("ShamanWings",
                                                  "Wings II",
                                                  wings_attack_hex.Description);
            wings_hex.AddComponents(Helpers.PrerequisiteClassLevel(shaman_class, 8),
                                    Helpers.PrerequisiteFeature(wings_attack_hex));

            draconic_resilence = hex_engine.createDraconicResilence("ShamanDraconicResilence",
                                                                    "Draconic Resilence",
                                                                    "he shaman grants a creature she touches some of the magically resilient nature of dragons, causing the creature to be immune to magical sleep effects for a number of rounds equal to the shaman’s level. At 7th level, the creature is also immune to paralysis for this duration. Once a creature has benefited from the draconic resilience hex, it cannot benefit from this hex again for 24 hours.");
            fury = hex_engine.createFury("ShamanFury",
                                         "Fury",
                                         "A shaman incites a creature within 30 feet into a primal fury. The target receives a +2 morale bonus on attack rolls and a +2 resistance bonus on saving throws against fear for a number of rounds equal to the shaman’s Wisdom modifier. At 8th and 16th levels, these bonuses increase by 1. Once a creature has benefited from the fury hex, it cannot benefit from it again for 24 hours.");
            secret = hex_engine.createSecret("ShamanSecret",
                                             "Secret",
                                             "he shaman receives one metamagic feat as a bonus feat. The shaman must meet the prerequisites for the feat.");
            intimidating_display = hex_engine.createIntimidatingDisplay("ShamanIntimidatingDisplay",
                                                                        "Intimidating Display",
                                                                        "The shaman can call upon some of the majesty and power of dragons to cow her enemies. The shaman gains Dazzling Display as a bonus feat, even if she does not meet the prerequisites, and she can use it even when not wielding a weapon.");
            chant = hex_engine.createCackle("ShamanChant",
                                            "Chant",
                                            "A shaman can chant as a move action. Any creature that is within 30 feet that is under the effects of the shaman’s charm, evil eye, fortune, fury, or misfortune hex has that effect’s duration extended by 1 round.",
                                            "", "", "", "", "ShamanChantToggleAbility");


            //additional witch hexes
            beast_of_ill_omen = hex_engine.createBeastOfIllOmen("ShamanBeastOfIllOmen",
                                                                Witch.beast_of_ill_omen.Name,
                                                                Witch.beast_of_ill_omen.Description,
                                                                "", "", "");
            slumber_hex = hex_engine.createSlumber("ShamanSlumber",
                                                   Witch.slumber_hex.Name,
                                                   Witch.slumber_hex.Description,
                                                   "", "", "");
            iceplant_hex = hex_engine.createIceplantHex("ShamanIcePlant",
                                                        Witch.iceplant_hex.Name,
                                                        Witch.iceplant_hex.Description,
                                                        "");

            murksight_hex = hex_engine.createMurksightHex("ShamanMurkSight",
                                                          Witch.murksight_hex.Name,
                                                          Witch.murksight_hex.Description,
                                                          "");
            ameliorating = hex_engine.createAmeliorating("ShamanAmeliorating",
                                                         Witch.ameliorating.Name,
                                                         Witch.ameliorating.Description,
                                                         "", "", "", "", "", "");
            summer_heat = hex_engine.createSummerHeat("ShamanSummerHeat",
                                                      Witch.summer_heat.Name,
                                                      Witch.summer_heat.Description,
                                                      "", "", "", "", "");
        }


        static void createHexSelections()
        {
            witch_hex_selection = Helpers.CreateFeatureSelection("ShamanWitchHexSelection",
                                                                 "Witch Hex",
                                                                 "The shaman selects any one hex normally available through the witch’s hex class feature. She treats her shaman level as her witch level when determining the powers and abilities of the hex. She uses her Wisdom modifier in place of her Intelligence modifier for the hex. She cannot select major hexes or grand hexes using this ability. The shaman cannot select a witch hex that has the same name as a shaman hex.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.None);
            witch_hex_selection.AddComponent(Helpers.PrerequisiteNoFeature(witch_hex_selection));
            witch_hex_selection.HideInCharacterSheetAndLevelUp = true;

            witch_hex_selection.AllFeatures = new BlueprintFeature[] { beast_of_ill_omen, slumber_hex, iceplant_hex, murksight_hex, ameliorating, summer_heat };

            hex_selection = Helpers.CreateFeatureSelection("ShamanHexSelection",
                                                           "Hex",
                                                           "A shaman learns a number of magical tricks, called hexes, which grant her powers or weaken foes. At 2nd level, a shaman learns one hex. At 4th, 8th, 10th, 12th, 16th, 18th, and 20th level, the shaman learns new hexes. A shaman can select from any of the following hexes or from any of the hexes listed in the description of her chosen spirit. A shaman cannot select a hex more than once unless noted otherwise.\n"
                                                           + "Using a hex is a standard action that doesn’t provoke an attack of opportunity unless otherwise noted. The saving throw DC to resist a hex is equal to 10 + 1/2 the shaman’s level + the shaman’s Wisdom modifier.",
                                                           "",
                                                           null,
                                                           FeatureGroup.None);
            hex_selection.AllFeatures = new BlueprintFeature[] {healing, misfortune_hex, fortune_hex, evil_eye, ward, shapeshift, wings_attack_hex, wings_hex,
                                                                draconic_resilence, fury, secret, intimidating_display, chant, witch_hex_selection};


            wandering_hex_selection = Helpers.CreateFeatureSelection("WanderingHexSelection",
                                                           "Wandering Hex",
                                                           "At 6th level, a shaman can gain the use of one of the hexes possessed by either one of her spirits. \n"
                                                           + "At 14th level, a shaman can select another wandering hex. This ability otherwise functions as the hex class feature.",
                                                           "",
                                                           null,
                                                           FeatureGroup.None);
           
            foreach (var s in spirits)
            {
                hex_selection.AllFeatures = hex_selection.AllFeatures.AddToArray(s.Value.hex_selection);
                wandering_hex_selection.AllFeatures = wandering_hex_selection.AllFeatures.AddToArray(s.Value.wandering_hex_selection);
            }
        }


        static void createShamanOrisons()
        {
            var daze = library.Get<BlueprintAbility>("55f14bc84d7c85446b07a1b5dd6b2b4c");
            shaman_orisons = Common.createCantrips("ShamanOrisonsFeature",
                                                   "Orisons",
                                                   "Shamans can prepare a number of orisons, or 0-level spells. These spells are cast like any other spell, but they are not expended when cast and may be used again.",
                                                   daze.Icon,
                                                   "8051f445c28e451baf670036be2b6d8c",
                                                   shaman_class,
                                                   StatType.Wisdom,
                                                   shaman_class.Spellbook.SpellList.SpellsByLevel[0].Spells.ToArray());
        }


        static void createShamanFamiliar()
        {
            shaman_familiar = library.CopyAndAdd<BlueprintFeatureSelection>("363cab72f77c47745bf3a8807074d183", "ShamanSpiritAnimal", "dc6e4b01140349e39806ace27ed1140e");
            shaman_familiar.DlcType = Kingmaker.Blueprints.Root.DlcType.None;
            shaman_familiar.ComponentsArray = new BlueprintComponent[0];
            shaman_familiar.SetNameDescription("Spirit Animal",
                                               "At 1st level, a shaman forms a close bond with a spirit animal tied to her chosen spirit. This animal is her conduit to the spirit world, guiding her along the path to enlightenment. The animal also aids a shaman by granting her a special ability. A shaman must commune with her spirit animal each day to prepare her spells. While the spirit animal does not store the spells like a witch’s familiar does, the spirit animal serves as her conduit to divine power. If a shaman’s spirit animal is slain, she cannot prepare new spells or use her spirit magic class feature until the spirit animal is replaced.");
        }


        static void createShamanProficiencies()
        {
            shaman_proficiencies = library.CopyAndAdd<BlueprintFeature>("8c971173613282844888dc20d572cfc9", //cleric proficiencies
                                                                "ShamanProficiencies",
                                                                "abb7c5dfa46e487bae52cc62ec5e6998");
            shaman_proficiencies.SetName("Shaman Proficiencies");
            shaman_proficiencies.SetDescription("A shaman is proficient with all simple weapons, and with light and medium armor.");
            shaman_proficiencies.ReplaceComponent<AddFacts>(a => a.Facts = a.Facts.Take(3).ToArray()); //remove shield proficiency
        }


        static void createSpiritMagic()
        {
            spirit_magic_spellbook = Helpers.Create<BlueprintSpellbook>();
            spirit_magic_spellbook.Name = Helpers.CreateString("SpiritMagicSpellbook.Name", "Spirit Magic");
            spirit_magic_spellbook.name = "ShamanSpiritMagicSpellbook";
            library.AddAsset(spirit_magic_spellbook, "54ff6031e1ee46b3815cd6a51dcd17d1");

            spirit_magic_spellbook.SpellsPerDay = Common.createOneSpellSpellTable("SpiritMagicSpellsPerDaySpellTable", "") ;
            spirit_magic_spellbook.SpellsKnown = Common.createEmptySpellTable("SpiritMagicSpellsKnownSpellTable", "");
            spirit_magic_spellbook.Spontaneous = true;
            spirit_magic_spellbook.IsArcane = false;
            spirit_magic_spellbook.AllSpellsKnown = false;
            spirit_magic_spellbook.CanCopyScrolls = false;
            spirit_magic_spellbook.CastingAttribute = StatType.Wisdom;
            spirit_magic_spellbook.CharacterClass = shaman_class;
            spirit_magic_spellbook.CasterLevelModifier = 0;
            spirit_magic_spellbook.CantripsType = CantripsType.Cantrips;
            spirit_magic_spellbook.SpellsPerLevel = shaman_class.Spellbook.SpellsPerLevel;

            spirit_magic_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            spirit_magic_spellbook.SpellList.name = "SpiritMagicSpellList";
            library.AddAsset(spirit_magic_spellbook.SpellList, "ea339eba9a81419c9caacdda1c027d5d");
            spirit_magic_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < spirit_magic_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                spirit_magic_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);
            }

            spirit_magic_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.NoSpellsPerDaySacaling>());
            shaman_class.Spellbook.AddComponent(Helpers.Create<SpellbookMechanics.CompanionSpellbook>(c => c.spellbook = spirit_magic_spellbook));
            spirit_magic = Helpers.CreateFeature("SpiritMagicFeature",
                                                 "Spirit Magic",
                                                 "A shaman can spontaneously cast a limited number of spells per day beyond those she prepared ahead of time. She has one spell slot per day of each shaman spell level she can cast, not including orisons. She can choose these spells from the list of spells granted by her spirits (see the spirit class feature and the wandering spirit class feature) at the time she casts them. She can enhance these spells using any metamagic feat that she knows, using up a higher-level spell slot as required by the feat and increasing the time to cast the spell (see Spontaneous Casting and Metamagic Feats).",
                                                 "",
                                                 null,
                                                 FeatureGroup.None
                                                 );
        }
    }
}
