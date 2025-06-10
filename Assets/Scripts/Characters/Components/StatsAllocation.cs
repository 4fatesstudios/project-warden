using UnityEditor.SceneManagement;
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

        public int getVitality(){
            return vitality;
        }
        
        public void setVitality(int value){
            vitality = value;
        }

        public void addVitatlity(int value){
            vitality += value;
        }

        public int getStrength(){
            return strength;
        }
        
        public void setStrength(int value){
            strength = value;
        }

        public void addStrength(int value){
            strength += value;
        }

        public int getWisdom(){
            return wisdom;
        }
        
        public void setWisdom(int value){
            wisdom = value;
        }

        public void addWisdom(int value){
            wisdom += value;
        }

        public int getAgility(){
            return agility;
        }
        
        public void setAgility(int value){
            agility = value;
        }

        public void addAgility(int value){
            agility += value;
        }

        public int getLuck(){
            return luck;
        }
        
        public void setLuck(int value){
            luck = value;
        }

        public void addLuck(int value){
            luck += value;
        }
        
    }
}