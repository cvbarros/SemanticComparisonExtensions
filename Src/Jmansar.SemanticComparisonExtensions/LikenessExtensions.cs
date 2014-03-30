﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Ploeh.SemanticComparison;
using Ploeh.SemanticComparison.Fluent;

namespace Jmansar.SemanticComparisonExtensions
{
    public static class LikenessExtensions
    {

        public static Likeness<TType, TType> WithInnerLikeness<TType, TProperty>(this Likeness<TType, TType> likeness,
            Expression<Func<TType, TProperty>> propertyPicker,
            Func<Likeness<TProperty, TProperty>, Likeness<TProperty, TProperty>>
                likenessDefFunc = null)
            where TProperty : class
        {
            return likeness.WithInnerLikeness(propertyPicker, propertyPicker, likenessDefFunc);
        }


        public static Likeness<TSource, TDestination> WithInnerLikeness<TSource, TDestination, TSourceProperty, TDestinationProperty>(
            this Likeness<TSource, TDestination> likeness,
            Expression<Func<TDestination, TDestinationProperty>> propertyPicker,
            Expression<Func<TSource, TSourceProperty>> sourcePropertyPicker,
            Func<Likeness<TSourceProperty, TDestinationProperty>, Likeness<TSourceProperty, TDestinationProperty>>
                likenessDefFunc = null) 
            where TSourceProperty: class 
            where TDestinationProperty: class
        {
            return WithInnerSpecificLikeness(likeness, propertyPicker, sourcePropertyPicker, likenessDefFunc);
        }

        public static Likeness<TType, TType> WithInnerSpecificLikeness<TType, TProperty, TPropertySubType>(
                this Likeness<TType, TType> likeness, Expression<Func<TType, TProperty>> propertyPicker, 
                Func<Likeness<TPropertySubType, TPropertySubType>, Likeness<TPropertySubType, TPropertySubType>> likenessDefFunc)
            where TProperty : class
            where TPropertySubType : class, TProperty

        {
            return WithInnerSpecificLikeness(likeness, propertyPicker, propertyPicker, likenessDefFunc);
        }


        public static Likeness<TSource, TDestination> WithInnerSpecificLikeness<TSource, TDestination, TSourceProperty, TDestinationProperty,
            TSourcePropertySubType, TDestinationPropertySubType>(
            this Likeness<TSource, TDestination> likeness,
            Expression<Func<TDestination, TDestinationProperty>> propertyPicker,
            Expression<Func<TSource, TSourceProperty>> sourcePropertyPicker,
            Func<Likeness<TSourcePropertySubType, TDestinationPropertySubType>, Likeness<TSourcePropertySubType, TDestinationPropertySubType>>
                likenessDefFunc)
            where TSourceProperty : class
            where TDestinationProperty : class
            where TSourcePropertySubType : class, TSourceProperty
            where TDestinationPropertySubType : class, TDestinationProperty

        {
            return likeness.With(propertyPicker)
                .EqualsWhen((s, d) =>
                {
                    var sourceVal = sourcePropertyPicker.Compile().Invoke(s);
                    var destVal = propertyPicker.Compile().Invoke(d);
                    if (sourceVal == null && destVal == null)
                    {
                        return true;
                    }

                    if (sourceVal == null || destVal == null)
                    {
                        return false;
                    }


                    var sourceValCast = sourceVal as TSourcePropertySubType;
                    if (sourceValCast == null)
                    {
                        throw new ArgumentException(
                            String.Format("Source property value is type of '{1}', cannot cast to '{0}'",
                                typeof(TSourcePropertySubType).FullName, sourceVal.GetType().FullName));
                    }

                    var destValCast = destVal as TDestinationPropertySubType;
                    if (destValCast == null)
                    {
                        // destination value has different type than destination type of inner likeness passed,
                        // so it is not equal
                        return false;
                    }

                    var innerLikeness = sourceValCast.AsSource().OfLikeness<TDestinationPropertySubType>();
                    if (likenessDefFunc != null)
                    {
                        innerLikeness = likenessDefFunc.Invoke(innerLikeness);
                    }

                    return innerLikeness.Equals(destValCast);
                });
        }
    }
}