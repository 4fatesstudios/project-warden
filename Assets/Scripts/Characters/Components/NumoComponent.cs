using System;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    public class NumoComponent
    {
        private int maxNumo;
        private int currentNumo;
    
        public NumoComponent(int maxNumo) {
            this.maxNumo = maxNumo;
            currentNumo = maxNumo;
        }
    
        public NumoComponent(int maxNumo, int currentNumo) {
            this.maxNumo = maxNumo;
            this.currentNumo = currentNumo;
        }
    
        public void UpdateNumo(int value) {
            currentNumo = Math.Clamp(currentNumo + value, 0, maxNumo);
        }
    
        public void UpdateMaxNumo(int value, bool heal=false) {
            var numoPercent = GetCurrentNumoPercentage();
            maxNumo = Math.Clamp(maxNumo + value, 0, maxNumo);
            currentNumo = maxNumo * numoPercent;
        }
    
        public int GetCurrentNumo() {
            return currentNumo;
        }
    
        public int GetMaxNumo() {
            return maxNumo;
        }
    
        public int GetCurrentNumoPercentage() {
            return (int)Mathf.Round((float)currentNumo / (float)maxNumo * 100f);
        }

        public int SetMaxNumo(int maxNumo){
            return this.maxNumo = maxNumo;
        }
    }
}