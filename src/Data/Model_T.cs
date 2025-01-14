﻿using DevZest.Data.Addons;
using DevZest.Data.Annotations;
using DevZest.Data.Annotations.Primitives;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace DevZest.Data
{
    /// <summary>
    /// Represents model with primary key.
    /// </summary>
    /// <typeparam name="T">The type of primary key.</typeparam>
    public abstract class Model<T> : Model
        where T : CandidateKey
    {
        /// <summary>
        /// Initializes a new intance of <see cref="Model{T}"/> class.
        /// </summary>
        protected Model()
        {
            AddDbTableConstraint(new DbPrimaryKey(this, GetDbPrimaryKeyName(), GetDbPrimaryKeyDescription(), true, () => PrimaryKey), true);
        }

        private T _primaryKey;
        /// <summary>
        /// Gets the primary key.
        /// </summary>
        public new T PrimaryKey
        {
            get { return LazyInitializer.EnsureInitialized(ref _primaryKey, () => CreatePrimaryKey()); }
        }

        /// <summary>
        /// Creates the primary key.
        /// </summary>
        /// <returns></returns>
        [CreateKey]
        protected abstract T CreatePrimaryKey();

        internal sealed override CandidateKey GetPrimaryKeyCore()
        {
            return this.PrimaryKey;
        }

        /// <summary>
        /// Gets the database primary key name.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetDbPrimaryKeyName()
        {
            var dbConstraintAttribute = typeof(T).GetTypeInfo().GetCustomAttribute<DbPrimaryKeyAttribute>();
            return dbConstraintAttribute == null ? "PK_%" : dbConstraintAttribute.Name;
        }

        /// <summary>
        /// Gets the database primary key description.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetDbPrimaryKeyDescription()
        {
            var dbConstraintAttribute = typeof(T).GetTypeInfo().GetCustomAttribute<DbPrimaryKeyAttribute>();
            return dbConstraintAttribute?.Description;
        }

        /// <summary>
        /// Matches this model with specified primary key.
        /// </summary>
        /// <param name="target">The specified primary key.</param>
        /// <returns>The result key mapping.</returns>
        public KeyMapping Match(T target)
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(PrimaryKey, target);
        }

        /// <summary>
        /// Matches this model with specified model.
        /// </summary>
        /// <param name="target">The specified model.</param>
        /// <returns>The result key mapping.</returns>
        public KeyMapping Match(Model<T> target)
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(PrimaryKey, target.PrimaryKey);
        }

        /// <summary>
        /// Matches this model with specified key projection.
        /// </summary>
        /// <param name="target">The specified key projection.</param>
        /// <returns>The result key mapping.</returns>
        public KeyMapping Match(Key<T> target)
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(PrimaryKey, target.PrimaryKey);
        }

        /// <summary>
        /// Registers a child model.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the child model is registered on.</typeparam>
        /// <typeparam name="TChildModel">The type of the child model.</typeparam>
        /// <param name="getter">The lambda expression of the child model getter.</param>
        /// <param name="relationshipGetter">Gets relationship between child model and parent model.</param>
        /// <returns>Mounter of the child model.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not a valid getter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relationshipGetter"/> is <see langword="null"/>.</exception>
        [PropertyRegistration]
        protected static Mounter<TChildModel> RegisterChildModel<TModel, TChildModel>(Expression<Func<TModel, TChildModel>> getter,
            Func<TChildModel, T> relationshipGetter)
            where TModel : Model<T>
            where TChildModel : Model, new()
        {
            getter.VerifyNotNull(nameof(getter));
            relationshipGetter.VerifyNotNull(nameof(relationshipGetter));
            Func<TModel, TChildModel> constructor = _ => new TChildModel();
            return s_childModelManager.Register(getter, a => CreateChildModel<TModel, TChildModel>(a, relationshipGetter, constructor), null);
        }

        private static TChildModel CreateChildModel<TModel, TChildModel>(Mounter<TModel, TChildModel> mounter,
            Func<TChildModel, T> relationshipGetter, Func<TModel, TChildModel> constructor)
            where TModel : Model<T>
            where TChildModel : Model, new()
        {
            var parentModel = mounter.Parent;
            TChildModel result = constructor(parentModel);
            var parentRelationship = relationshipGetter(result).UnsafeJoin(parentModel.PrimaryKey);
            var parentMappings = AppendColumnMappings(parentRelationship, null, result, parentModel);
            result.ConstructChildModel(parentModel, mounter.DeclaringType, mounter.Name, parentRelationship, parentMappings);
            return result;
        }

        private static IReadOnlyList<ColumnMapping> AppendColumnMappings<TChildModel, TParentModel>(IReadOnlyList<ColumnMapping> parentRelationship,
            Action<ColumnMapper, TChildModel, TParentModel> parentMappingsBuilderAction, TChildModel childModel, TParentModel parentModel)
            where TChildModel : Model
            where TParentModel : Model
        {
            if (parentMappingsBuilderAction == null)
                return parentRelationship;

            var parentMappingsBuilder = new ColumnMapper(childModel, parentModel);
            var parentMappings = parentMappingsBuilder.Build(x => parentMappingsBuilderAction(x, childModel, parentModel));

            var result = new ColumnMapping[parentRelationship.Count + parentMappings.Count];
            for (int i = 0; i < parentRelationship.Count; i++)
                result[i] = parentRelationship[i];
            for (int i = 0; i < parentMappings.Count; i++)
                result[i + parentRelationship.Count] = parentMappings[i];
            return result;
        }
    }
}
