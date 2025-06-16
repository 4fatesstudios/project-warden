using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    public class StatsAllocation
    {
        private int vitality;
        private int strength;
        private int wisdom;
        private int agility;   
        private int luck;
        

        public int GetVitality(){
            return vitality;
        }
        
        public void SetVitality(int value){
            vitality = value;
        }

        public void AddVitality(int value){
            vitality += value;
        }

        public int GetStrength(){
            return strength;
        }
        
        public void SetStrength(int value){
            strength = value;
        }

        public void AddStrength(int value){
            strength += value;
        }

        public int GetWisdom(){
            return wisdom;
        }
        
        public void SetWisdom(int value){
            wisdom = value;
        }

        public void AddWisdom(int value){
            wisdom += value;
        }

        public int GetAgility(){
            return agility;
        }
        
        public void SetAgility(int value){
            agility = value;
        }

        public void AddAgility(int value){
            agility += value;
        }

        public int GetLuck(){
            return luck;
        }
        
        public void SetLuck(int value){
            luck = value;
        }

        public void AddLuck(int value){
            luck += value;
        }
    }
}