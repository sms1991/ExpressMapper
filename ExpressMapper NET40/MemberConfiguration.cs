﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressMapper
{
    public class MemberConfiguration<T, TN> : IMemberConfiguration<T, TN>
    {
        private readonly ITypeMapper<T, TN>[] _typeMappers;
        public MemberConfiguration(ITypeMapper<T, TN>[] typeMappers)
        {
            _typeMappers = typeMappers;
        }

        public IMemberConfiguration<T, TN> InstantiateFunc(Func<T, TN> constructor)
        {
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.InstantiateFunc(constructor);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Instantiate(Expression<Func<T, TN>> constructor)
        {
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.Instantiate(constructor);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Before(Action<T, TN> beforeHandler)
        {
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.BeforeMap(beforeHandler);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> After(Action<T, TN> afterHandler)
        {
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.AfterMap(afterHandler);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Base<TBase>(Expression<Action<T, TN>> afterHandler) where TBase : class, new()
        {
            throw new NotImplementedException();
        }

        public IMemberConfiguration<T, TN> Member<TMember, TNMember>(Expression<Func<TN, TNMember>> dest, Expression<Func<T, TMember>> src)
        {
            if (dest == null)
            {
                throw new ArgumentNullException("dst");
            }

            var memberExpression = dest.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new Exception("MemberExpression should return one of the properties of destination class");
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;

            if (propertyInfo != null && !propertyInfo.CanWrite || (propertyInfo != null && propertyInfo.CanWrite && !propertyInfo.GetSetMethod(true).IsPublic))
            {
                Ignore(dest);
            }
            else
            {
                foreach (var typeMapper in _typeMappers)
                {
                    typeMapper.MapMember(dest, src);
                }
            }
            return this;
        }

        //Note: If specific destination address mapping has already been set then it will NOT add the mapping given.
        //This is used when flattening, which comes last in the stage, to ensure that the developer can override the flattening mappings
        public IMemberConfiguration<T, TN> MemberComputed(MemberExpression destExpression, Expression sourceExpression)
        {
            var propertyInfo = destExpression.Member as PropertyInfo;
            if (propertyInfo != null && !propertyInfo.CanWrite || (propertyInfo != null && propertyInfo.CanWrite && !propertyInfo.GetSetMethod(true).IsPublic))
            {
                foreach (var typeMapper in _typeMappers)
                {
                    typeMapper.Ignore(propertyInfo);
                }
            }
            else
            {
                foreach (var typeMapper in _typeMappers)
                {
                    typeMapper.MapMemberComputed(destExpression, sourceExpression);
                }
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Function<TMember, TNMember>(Expression<Func<TN, TNMember>> dest, Func<T, TMember> src)
        {
            var memberExpression = dest.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new Exception("MemberExpression should return one of the properties of destination class");
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;

            if (propertyInfo != null && !propertyInfo.CanWrite && !propertyInfo.GetSetMethod(true).IsPublic)
            {
                Ignore(dest);
            }
            else
            {
                foreach (var typeMapper in _typeMappers)
                {
                    typeMapper.MapFunction(dest, src);
                }
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Ignore<TMember>(Expression<Func<TN, TMember>> dest)
        {
            if (dest == null)
            {
                throw new ArgumentNullException("dst");
            }

            if (!(dest.Body is MemberExpression))
            {
                throw new Exception("MemberExpression should return one of the properties of destination class");
            }
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.Ignore(dest);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Value<TNMember>(Expression<Func<TN, TNMember>> dest, TNMember value)
        {
            if (!(dest.Body is MemberExpression))
            {
                throw new Exception("MemberExpression should return one of the properties of destination class");
            }
            return Member(dest, x => value);
        }

        public IMemberConfiguration<T, TN> CaseSensitive(bool caseSensitive)
        {
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.CaseSensetiveMemberMap(caseSensitive);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> CompileTo(CompilationTypes compilationType)
        {
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.CompileTo(compilationType);
            }
            return this;
        }
    }
}
