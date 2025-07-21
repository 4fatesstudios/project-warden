using System.Collections.Generic;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Databases
{
    [CreateAssetMenu(fileName = "NewDatabase", menuName = "Databases/Skill Database")]
    public class SkillDatabase : ScriptableObject {
        private static SkillDatabase _instance;

        public static SkillDatabase Instance {
            get {
                if (_instance == null)
                    _instance = Resources.Load<SkillDatabase>("Databases/SkillDatabase");
                if (_instance == null)
                    Debug.LogError("SkillDatabase asset not found in Resources/Databases/SkillDatabase");
                return _instance;
            }
        }

        [SerializeField] private List<Skill> skills = new();
        private Dictionary<string, Skill> skillLookup;
        
        public IReadOnlyList<Skill> Skills => skills.AsReadOnly();

        private void OnEnable() {
            BuildLookup();
        }

        private void BuildLookup() {
            skillLookup = new Dictionary<string, Skill>();
            foreach (var skill in skills) {
                if (skillLookup.ContainsKey(skill.SkillID)) {
                    Debug.LogError($"Duplicate SkillID detected: {skill.SkillID} in {skill.name}");
                    continue;
                }
                if (skill != null && !string.IsNullOrEmpty(skill.SkillID))
                    skillLookup[skill.SkillID] = skill;
            }
        }

        public Skill GetSkillById(string skillID) {
            if (skillLookup == null)
                BuildLookup();
            
            skillLookup.TryGetValue(skillID, out var skill);
            return skill;
        }
    }
}