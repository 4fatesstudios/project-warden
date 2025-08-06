using System;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Enums.Tooltips.Attributes {
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumTooltipAttribute : PropertyAttribute {
        public Type EnumType { get; }
        public Type TooltipProviderType { get; }

        public EnumTooltipAttribute(Type enumType, Type tooltipProviderType) {
            EnumType = enumType;
            TooltipProviderType = tooltipProviderType;
        }
    }
}