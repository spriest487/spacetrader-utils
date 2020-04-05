using System;
using System.Collections.Generic;

namespace SpaceTrader.Util {
    public class BindAttribute : Attribute {
        public BindAttribute() {
            this.Types = new Type[0];
        }

        public BindAttribute(params Type[] types) {
            this.Types = types;
        }

        public Type[] Types { get; }

        public static IEnumerable<Type> FindBindTypes(Type ty) {
            var attrs = ty.GetCustomAttributes(typeof(BindAttribute), true);

            foreach (BindAttribute attr in attrs) {
                foreach (var bindTy in attr.Types) {
                    yield return bindTy;
                }
            }
        }
    }
}