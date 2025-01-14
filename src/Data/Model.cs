﻿using DevZest.Data.Addons;
using DevZest.Data.Annotations;
using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace DevZest.Data
{
    /// <summary>
    /// Base class of your busines model objects.
    /// </summary>
    public abstract partial class Model : ModelMember, IModels
    {
        #region RegisterColumn

        internal static MounterManager<Model, Column> s_columnManager = new MounterManager<Model, Column>();

        /// <summary>
        /// Registers a new column which has a default constructor.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the column is registered on.</typeparam>
        /// <typeparam name="TColumn">The type of the column.</typeparam>
        /// <param name="getter">The lambda expression of the column getter.</param>
        /// <returns>Mounter of the column.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not a valid getter.</exception>
        [PropertyRegistration]
        protected static Mounter<TColumn> RegisterColumn<TModel, TColumn>(Expression<Func<TModel, TColumn>> getter)
            where TModel : Model
            where TColumn : Column, new()
        {
            var initializer = getter.Verify(nameof(getter));

            return s_columnManager.Register(getter, mounter => CreateColumn(mounter, initializer));
        }

        private static T CreateColumn<TModel, T>(Mounter<TModel, T> mounter, Action<T> initializer)
            where TModel : Model
            where T : Column, new()
        {
            var result = Column.Create<T>(mounter.DeclaringType, mounter.Name);
            result.Construct(mounter.Parent, mounter.DeclaringType, mounter.Name, ColumnKind.General, null, initializer);
            return result;
        }

        /// <summary>
        /// Registers a column from existing column mounter.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the column is registered on.</typeparam>
        /// <typeparam name="TColumn">The type of the column.</typeparam>
        /// <param name="getter">The lambda expression of the column getter.</param>
        /// <param name="fromMounter">The existing column mounter.</param>
        /// <returns>Mounter of the column.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not an valid getter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="fromMounter"/> is null.</exception>
        [PropertyRegistration]
        protected static void RegisterColumn<TModel, TColumn>(Expression<Func<TModel, TColumn>> getter, Mounter<TColumn> fromMounter)
            where TModel : Model
            where TColumn : Column, new()
        {
            var initializer = getter.Verify(nameof(getter));
            fromMounter.VerifyNotNull(nameof(fromMounter));

            var result = s_columnManager.Register(getter, mounter => CreateColumn(mounter, fromMounter, initializer));
            result.OriginalDeclaringType = fromMounter.OriginalDeclaringType;
            result.OriginalName = fromMounter.OriginalName;
        }

        private static TColumn CreateColumn<TModel, TColumn>(Mounter<TModel, TColumn> mounter, Mounter<TColumn> fromMounter, Action<TColumn> initializer)
            where TModel : Model
            where TColumn : Column, new()
        {
            var result = Column.Create<TColumn>(fromMounter.OriginalDeclaringType, fromMounter.OriginalName);
            result.Construct(mounter.Parent, mounter.DeclaringType, mounter.Name, ColumnKind.General, fromMounter.Initializer, initializer);
            return result;
        }

        #endregion

        #region RegisterColumnList

        static MounterManager<Model, ColumnList> s_columnListManager = new MounterManager<Model, ColumnList>();

        /// <summary>
        /// Registers a column list.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the column is registered on.</typeparam>
        /// <typeparam name="T">The type of the column contained by the column list.</typeparam>
        /// <param name="getter">The lambda expression of the column list getter.</param>
        /// <returns>Mounter of the column list.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not a valid getter.</exception>
        [PropertyRegistration]
        protected static void RegisterColumnList<TModel, T>(Expression<Func<TModel, ColumnList<T>>> getter)
            where TModel : Model
            where T : Column
        {
            getter.VerifyNotNull(nameof(getter));

            s_columnListManager.Register(getter, a => CreateColumnList(a), null);
        }

        private static ColumnList<T> CreateColumnList<TModel, T>(Mounter<TModel, ColumnList<T>> mounter)
            where TModel : Model
            where T : Column
        {
            var result = new ColumnList<T>();
            var parent = mounter.Parent;
            result.ConstructModelMember(parent, mounter.DeclaringType, mounter.Name);
            parent.Add(result);
            return result;
        }

        private List<ColumnList> _columnLists;
        internal IReadOnlyList<ColumnList> ColumnLists
        {
            get
            {
                if (_columnLists == null)
                    return Array.Empty<ColumnList>();
                else
                    return _columnLists;
            }
        }

        private void Add(ColumnList columnList)
        {
            if (_columnLists == null)
                _columnLists = new List<ColumnList>();
            _columnLists.Add(columnList);
        }

        #endregion

        #region RegisterProjection

        static MounterManager<Model, Projection> s_projectionManager = new MounterManager<Model, Projection>();

        /// <summary>Registers a new <see cref="Projection"/>.</summary>
        /// <typeparam name="TModel">The type of model which the column is registered on.</typeparam>
        /// <typeparam name="TProjection">The type of the <see cref="Projection"/>.</typeparam>
        /// <param name="getter">The lambda expression of the <see cref="Projection"/> getter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not a valid getter.</exception>
        [PropertyRegistration]
        protected static void RegisterProjection<TModel, TProjection>(Expression<Func<TModel, TProjection>> getter)
            where TModel : Model
            where TProjection : Projection, new()
        {
            getter.VerifyNotNull(nameof(getter));
            s_projectionManager.Register(getter, mounter => CreateProjection(mounter));
        }

        private List<Projection> _projections;
        /// <summary>
        /// Gets the <see cref="Projection"/> objects created by this model.
        /// </summary>
        protected internal IReadOnlyList<Projection> Projections
        {
            get
            {
                if (_projections == null)
                    return Array.Empty<Projection>();
                else
                    return _projections;
            }
        }

        /// <summary>
        /// Gets the <see cref="Projection"/> objects created by this model.
        /// </summary>
        /// <returns>The <see cref="Projection"/> objects created by this model.</returns>
        public IReadOnlyList<Projection> GetProjections()
        {
            return Projections;
        }

        private void Add(Projection projection)
        {
            Debug.Assert(projection != null);
            if (_projections == null)
                _projections = new List<Projection>();
            _projections.Add(projection);
        }

        private static TProjection CreateProjection<TModel, TProjection>(Mounter<TModel, TProjection> mounter)
            where TModel : Model
            where TProjection : Projection, new()
        {
            var parent = mounter.Parent;
            var result = new TProjection();
            result.ConstructChildProjection(parent, mounter.DeclaringType, mounter.Name);
            result.ColumnOrdinalOffset = parent.OwnerModel.TotalColumnCount;
            parent.Add(result);
            return result;
        }

        private void ConstructChildProjection(Model parentModel, Type declaringType, string name)
        {
            this.ConstructModelMember(parentModel, declaringType, name);
            _ownerModel = parentModel.OwnerModel;
            Depth = ParentModel.Depth;
            Namespace = name.GetFullName(parentModel.Namespace);
        }

        internal string Namespace { get; private set; }

        #endregion

        #region RegisterChildModel

        internal static MounterManager<Model, Model> s_childModelManager = new MounterManager<Model, Model>();

        /// <summary>
        /// Registers a child model.
        /// </summary>
        /// <typeparam name="TModel">Type of the model.</typeparam>
        /// <typeparam name="TChildModel">Type of the child model.</typeparam>
        /// <param name="getter">The lambda expression of the child model getter.</param>
        /// <param name="constructor">The delegate to create the child model.</param>
        /// <returns>Mounter of the child model.</returns>
        [PropertyRegistration]
        protected static Mounter<TChildModel> RegisterChildModel<TModel, TChildModel>(Expression<Func<TModel, TChildModel>> getter, Func<TModel, TChildModel> constructor = null)
            where TModel : Model
            where TChildModel : Model, new()
        {
            getter.VerifyNotNull(nameof(getter));
            if (constructor == null)
                constructor = _ => new TChildModel();
            return s_childModelManager.Register(getter, mounter => CreateChildModel(mounter, constructor), null);
        }

        private static TChildModel CreateChildModel<TModel, TChildModel>(Mounter<TModel, TChildModel> mounter, Func<TModel, TChildModel> constructor)
            where TModel : Model
            where TChildModel : Model, new()
        {
            var parentModel = mounter.Parent;
            TChildModel result = constructor(parentModel);
            result.ConstructChildModel(parentModel, mounter.DeclaringType, mounter.Name);
            return result;
        }

        internal void ConstructChildModel(Model parentModel, Type declaringType, string name)
        {
            ConstructChildModel(parentModel, declaringType, name, Array.Empty<ColumnMapping>(), Array.Empty<ColumnMapping>());
        }

        internal void ConstructChildModel(Model parentModel, Type declaringType, string name, IReadOnlyList<ColumnMapping> parentRelationship, IReadOnlyList<ColumnMapping> parentMappings)
        {
            this.ConstructModelMember(parentModel, declaringType, name);

            ParentModel.ChildModels.Add(this);
            Depth = ParentModel.Depth + 1;

            Debug.Assert(parentMappings != null);
            Debug.Assert(parentRelationship != null);
            ParentRelationship = parentRelationship;
            ParentMappings = parentMappings;
            InitChildColumns();
        }

        private void InitChildColumns()
        {
            var parentMappings = ParentMappings;
            for (int i = 0; i < parentMappings.Count; i++)
            {
                var mapping = parentMappings[i];
                mapping.Source.InitAsChild(mapping.Target);
            }
        }

        /// <summary>
        /// Gets the column mappings between its parent model and this model.
        /// </summary>
        internal IReadOnlyList<ColumnMapping> ParentRelationship { get; private set; }

        internal IReadOnlyList<ColumnMapping> ParentMappings { get; private set; }

        #endregion

        #region RegisterLocalColumn

        static MounterManager<Model, Column> s_localColumnManager = new MounterManager<Model, Column>();

        /// <summary>
        /// Registers a new local column.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the column is registered on.</typeparam>
        /// <typeparam name="T">The data type of the local column.</typeparam>
        /// <param name="getter">The lambda expression of the column getter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not a valid getter.</exception>
        [PropertyRegistration]
        protected static void RegisterLocalColumn<TModel, T>(Expression<Func<TModel, LocalColumn<T>>> getter)
            where TModel : Model
        {
            var initializer = getter.Verify(nameof(getter));
            s_localColumnManager.Register(getter, mounter => CreateLocalColumn(mounter, initializer));
        }

        private List<Column> _localColumns;
        /// <summary>
        /// Gets local columns owned by this model.
        /// </summary>
        protected internal IReadOnlyList<Column> LocalColumns
        {
            get
            {
                if (_localColumns == null)
                    return Array.Empty<Column>();
                else
                    return _localColumns;
            }
        }

        private static LocalColumn<T> CreateLocalColumn<TModel, T>(Mounter<TModel, LocalColumn<T>> mounter, Action<LocalColumn<T>> initializer)
            where TModel : Model
        {
            var result = new LocalColumn<T>();
            result.Construct(mounter.Parent, mounter.DeclaringType, mounter.Name, ColumnKind.Local, null, initializer);
            return result;
        }

        /// <summary>
        /// Creates a local column.
        /// </summary>
        /// <typeparam name="T">Data type of the local column.</typeparam>
        /// <returns>The created local column.</returns>
        protected LocalColumn<T> CreateLocalColumn<T>()
        {
            VerifyDesignMode();
            var result = new LocalColumn<T>();
            result.Construct(this, GetType(), null, ColumnKind.Local, null, null);
            return result;
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="Model"/> class.
        /// </summary>
        protected Model()
        {
            Columns = new ColumnCollection(this);
            ChildModels = new ModelCollection(this);

            s_columnManager.Mount(this);
            s_projectionManager.Mount(this);
            s_localColumnManager.Mount(this);
            s_columnListManager.Mount(this);
            PerformConstructing();
        }

        internal override void ConstructModelMember(Model parentModel, Type declaringType, string name)
        {
            base.ConstructModelMember(parentModel, declaringType, name);
            _rootModel = ParentModel.RootModel;
        }

        internal int Depth { get; private set; }

        private Model _rootModel;
        internal Model RootModel
        {
            get { return _rootModel ?? this; }
        }

        internal Model ParentModel
        {
            get { return Parent; }
        }

        private Model _ownerModel;
        internal Model OwnerModel
        {
            get { return _ownerModel ?? this; }
        }

        /// <summary>
        /// Gets a value indicates whether child models are initialized.
        /// </summary>
        /// <remarks>Unlike <see cref="Column"/> and <see cref="ColumnList"/>,
        /// child models are not initialized by default. This design decision is to deal with the situation when recursive child models registered.
        /// <see cref="EnsureInitialized()"/> will be called automatically when creating the first <see cref="DataRow"/> the query builder.
        /// </remarks>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Ensures child models are initialized.
        /// </summary>
        /// <remarks>Unlike <see cref="Column"/> and <see cref="ColumnList"/>,
        /// child models are not initialized by default. This design decision is to deal with the situation when recursive child models registered.
        /// <see cref="EnsureInitialized()"/> will be called automatically when creating the first <see cref="DataRow"/>.
        /// </remarks>
        protected internal void EnsureInitialized()
        {
            if (IsInitialized)
                return;
            EnsureInitialized(true);
        }

#if !DEPLOY
        internal
#else
        private
#endif
        void EnsureInitialized(bool verifyDataSource)
        {
            if (verifyDataSource &&  DataSource == null)
                throw new InvalidOperationException(DiagnosticMessages.Model_EnsureInitializedNullDataSource);

            s_childModelManager.Mount(this);
            PerformChildModelsMounted();
            if (DataSource != null && DataSource.Kind == DataSourceKind.DataSet)
            {
                foreach (var model in ChildModels)
                {
                    var modelType = model.GetType();
                    var invoker = s_createDataSetInvokers.GetOrAdd(modelType, t => BuildCreateDataSetInvoker(t));
                    invoker(model);
                }
                PerformChildDataSetsCreated();
                DataSetContainer.MergeComputations(this);
            }
            InitColumnValueManager();
            IsInitialized = true;
            PerformInitialized();
        }

        private void InitColumnValueManager()
        {
            InitValueManager(Columns);
            InitValueManager(LocalColumns);
            foreach (var projection in Projections)
                projection.InitColumnValueManager();
        }

        private static void InitValueManager(IReadOnlyList<Column> columns)
        {
            for (int i = 0; i < columns.Count; i++)
                columns[i].InitValueManager();
        }

        private void PerformConstructing()
        {
            ModelAttribute.WireupAttributes(this, ModelWireupEvent.Constructing);
            OnConstructing();
        }

        /// <summary>
        /// Raises <see cref="Constructing"/> event.
        /// </summary>
        protected virtual void OnConstructing()
        {
            Constructing(this, EventArgs.Empty);
        }

        private void PerformInitializing()
        {
            ModelAttribute.WireupAttributes(this, ModelWireupEvent.Initializing);
            OnInitializing();
        }

        /// <summary>
        /// Raises <see cref="Initializing"/> event.
        /// </summary>
        protected virtual void OnInitializing()
        {
            Initializing(this, EventArgs.Empty);
        }

        private void PerformChildModelsMounted()
        {
            ModelAttribute.WireupAttributes(this, ModelWireupEvent.ChildModelsMounted);
            OnChildModelsMounted();
        }

        /// <summary>
        /// Raises <see cref="ChildModelsMounted"/> event.
        /// </summary>
        protected virtual void OnChildModelsMounted()
        {
            ChildModelsMounted(this, EventArgs.Empty);
        }

        private void PerformChildDataSetsCreated()
        {
            ModelAttribute.WireupAttributes(this, ModelWireupEvent.ChildDataSetsCreated);
            OnChildDataSetsCreated();
        }

        /// <summary>
        /// Raises <see cref="ChildDataSetsCreated"/> event.
        /// </summary>
        protected virtual void OnChildDataSetsCreated()
        {
            ChildDataSetsCreated(this, EventArgs.Empty);
        }

        private void PerformInitialized()
        {
            ModelAttribute.WireupAttributes(this, ModelWireupEvent.Initialized);
            OnInitialized();
        }

        /// <summary>
        /// Raises <see cref="Initialized"/> event.
        /// </summary>
        protected virtual void OnInitialized()
        {
            Initialized(this, EventArgs.Empty);
        }

        private static ConcurrentDictionary<Type, Action<Model>> s_createDataSetInvokers = new ConcurrentDictionary<Type, Action<Model>>();

        private static DataSet<T> _CreateDataSet<T>(Model model)
            where T : Model, new()
        {
            return DataSet<T>.Create((T)model);
        }

        private static Action<Model> BuildCreateDataSetInvoker(Type modelType)
        {
            var methodInfo = typeof(Model).GetStaticMethodInfo(nameof(_CreateDataSet));
            methodInfo = methodInfo.MakeGenericMethod(modelType);
            var paramModel = Expression.Parameter(typeof(Model), methodInfo.GetParameters()[0].Name);
            var call = Expression.Call(methodInfo, paramModel);
            return Expression.Lambda<Action<Model>>(call, paramModel).Compile();
        }

        /// <summary>
        /// Gets the columns created by this <see cref="Model"/>.
        /// </summary>
        protected internal ColumnCollection Columns { get; private set; }

        internal int Add(Column column)
        {
            Debug.Assert(column != null);
            if (column.IsLocal)
            {
                if (_localColumns == null)
                    _localColumns = new List<Column>();
                _localColumns.Add(column);
                return _localColumns.Count - 1;
            }
            else
            {
                Columns.Add(column);
                return Columns.Count - 1;
            }
        }

        internal ModelCollection ChildModels { get; private set; }

        /// <summary>
        /// Gets the child models created by this <see cref="Model"/>.
        /// </summary>
        /// <returns></returns>
        public ModelCollection GetChildModels()
        {
            return ChildModels;
        }

        private List<IValidator> _validators;
        /// <summary>
        /// Gets the validators owned by this model.
        /// </summary>
        protected internal List<IValidator> Validators
        {
            get { return _validators ?? (_validators = new List<IValidator>()); }
        }

        /// <summary>
        /// Validates specified DataRow.
        /// </summary>
        /// <param name="dataRow">The specified DataRow.</param>
        /// <returns>The data validation errors.</returns>
        protected internal virtual IDataValidationErrors Validate(DataRow dataRow)
        {
            var result = DataValidationErrors.Empty;
            foreach (var validator in Validators)
                result = Merge(result, validator.Validate(dataRow));
            return result;
        }

        private static IDataValidationErrors Merge(IDataValidationErrors result, IDataValidationErrors validationErrors)
        {
            if (validationErrors != null)
            {
                foreach (var validationMessage in validationErrors)
                    result = result.Add(validationMessage);
            }
            return result;
        }

        /// <summary>
        /// Gets the primary key of this model.
        /// </summary>
        public CandidateKey PrimaryKey
        {
            get { return GetPrimaryKeyCore(); }
        }

        internal virtual CandidateKey GetPrimaryKeyCore()
        {
            return null;
        }

        /// <summary>
        /// Gets the DataSource which owns this model.
        /// </summary>
        protected internal DataSource DataSource { get; private set; }

        internal void SetDataSource(DataSource dataSource)
        {
            Debug.Assert(dataSource != null);
            Debug.Assert(DataSource == null);

            bool isDataSet = dataSource is DataSet;

            Columns.InitDbColumnNames();
            if (!isDataSet)
                PerformInitializing();
            DataSource = dataSource;
            DataSet = dataSource as DataSet;
            if (DataSet != null && RootModel == this)
                _dataSetContainer = new DataSetContainer();
            if (isDataSet)
                PerformInitializing();
        }

        /// <inheritdoc />
        protected internal sealed override bool DesignMode
        {
            get
            {
                if (DataSource is IDbTable dbTable)
                    return dbTable.DesignMode;

                if (DataSource == null)
                    return true;
                if (DataSource.Kind != DataSourceKind.DataSet)
                    return false;
                return !IsInitialized;
            }
        }

        /// <summary>
        /// Gets the DataSet which owns this model.
        /// </summary>
        public DataSet DataSet { get; private set; }

        private int _ordinal = -1;
        internal int Ordinal
        {
            get { return _ordinal; }
            set { _ordinal = value; }
        }

#region IModels
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IModels.Contains(Model model)
        {
            return model == this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<Model>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<Model> IEnumerable<Model>.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IModels.IsSealed
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IModels IModels.Seal()
        {
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IModels IModels.Add(Model value)
        {
            value.VerifyNotNull(nameof(value));
            if (value == this)
                return this;
            return Models.New(this, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IModels IModels.Remove(Model value)
        {
            value.VerifyNotNull(nameof(value));
            if (value == this)
                return Models.Empty;
            else
                return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IModels IModels.Clear()
        {
            return Models.Empty;
        }
#endregion

        internal Model Clone(bool setDataSource)
        {
            Model result = (Model)Activator.CreateInstance(this.GetType());
            result.InitializeClone(this, setDataSource);
            return result;
        }

        internal Action<Model> Initializer { get; set; }

        internal void InitializeClone(Model prototype, bool setDataSource)
        {
            Debug.Assert(prototype != null && prototype != this);
            InitializeColumnLists(prototype);
            if (prototype.Initializer != null)
            {
                Initializer = prototype.Initializer;
                Initializer(this);
            }
            if (setDataSource && prototype.DataSource != null)
                SetDataSource(prototype.DataSource);
        }

        private void InitializeColumnLists(Model prototype)
        {
            var properties = s_columnListManager.GetAll(this.GetType());
            foreach (var property in properties)
            {
                var columnList = property.GetInstance(this);
                var sourceColumnList = property.GetInstance(prototype);
                columnList.Initialize(sourceColumnList);
            }
        }

        private IIndexConstraint _clusteredIndex;
        private IIndexConstraint ClusteredIndex
        {
            get { return _clusteredIndex; }
            set
            {
                if (_clusteredIndex != null && _clusteredIndex != value)
                    throw new InvalidOperationException(DiagnosticMessages.Model_MultipleClusteredIndex(_clusteredIndex.SystemName));
                else
                    _clusteredIndex = value;
            }
        }

        internal void ChangeClusteredIndexAsNonClustered()
        {
            if (_clusteredIndex == null)
                return;

            _clusteredIndex.AsNonClustered();
            _clusteredIndex = null;
        }

        internal void AddDbTableConstraint(DbTableConstraint constraint, bool overwritable)
        {
            Debug.Assert(constraint != null);

            if (!overwritable && ContainsAddon(((IAddon)constraint).Key))
                throw new InvalidOperationException(DiagnosticMessages.Model_DuplicateConstraintName(constraint.SystemName));

            if (constraint is IIndexConstraint index && index.IsClustered)
                ClusteredIndex = index;

            this.AddOrUpdate(constraint);
        }

        internal void AddDbIndex(DbIndex index)
        {
            Debug.Assert(index != null);

            if (ContainsAddon(((IAddon)index).Key))
                throw new InvalidOperationException(DiagnosticMessages.Model_DuplicateIndexName(index.Name));

            if (index.IsClustered)
                ClusteredIndex = index;

            this.AddOrUpdate(index);
        }

        /// <summary>
        /// Gets the database alias of this model.
        /// </summary>
        protected internal virtual string DbAlias
        {
            get { return this.GetType().Name; }
        }

        private sealed class ModelMemberCollection : KeyedCollection<string, ModelMember>
        {
            protected override string GetKeyForItem(ModelMember item)
            {
                return item.Name;
            }
        }

        private ModelMemberCollection _members = new ModelMemberCollection();
        internal void AddMember(ModelMember member)
        {
            Debug.Assert(member != null);
            Debug.Assert(!string.IsNullOrEmpty(member.Name));
            _members.Add(member);
        }

        private bool ContainsMember(string memberName)
        {
            return string.IsNullOrEmpty(memberName) ? false : _members.Contains(memberName);
        }

        internal ModelMember this[string memberName]
        {
            get { return ContainsMember(memberName) ? _members[memberName] : null; }
        }

        /// <summary>
        /// Gets identity definition of the table.
        /// </summary>
        /// <param name="isTempTable"><see langword="true"/> for temporary table, otherwise permanent table.</param>
        /// <returns>The identity definition.</returns>
        public Identity GetIdentity(bool isTempTable)
        {
            return (Identity)GetAddon(isTempTable ? typeof(TempTableIdentity) : typeof(Identity));
        }

        internal const string SYS_ROW_ID_COL_NAME = "sys_row_id";
        private const string SYS_PARENT_ROW_ID_COL_NAME = "sys_parent_row_id";

        internal void AddTempTableIdentity()
        {
            var identityColumn = AddSysRowIdColumn(false);
            var identity = identityColumn.SetTempTableIdentity(1, 1);

            var primaryKeyConstraint = GetAddon<DbPrimaryKey>();
            if (primaryKeyConstraint == null)
                AddDbTableConstraint(new DbPrimaryKey(this, null, null, true, () => GetIdentityOrderByList(identity)), false);
            else
            {
                ChangeClusteredIndexAsNonClustered();
                AddDbTableConstraint(new DbUniqueConstraint(null, null, true, GetIdentityOrderByList(identity)), false);
            }
        }

        private static IList<ColumnSort> GetIdentityOrderByList(Identity identity)
        {
            return new ColumnSort[] { new ColumnSort(identity.Column, identity.Increment > 0 ? SortDirection.Ascending : SortDirection.Descending) };
        }

        internal DbFromClause FromClause
        {
            get { return !(DataSource is IDbSet dbSet) ? null : dbSet.FromClause; }
        }

        private _Int32 AddSysRowIdColumn(bool isParent)
        {
            _Int32 result = new _Int32();
            result.Initialize(this, this.GetType(), isParent ? SYS_PARENT_ROW_ID_COL_NAME : SYS_ROW_ID_COL_NAME,
                isParent ? ColumnKind.SystemParentRowId : ColumnKind.SystemRowId, null);
            return result;
        }

        private class SysRowId : IAddon
        {
            public SysRowId(_Int32 column)
            {
                Column = column;
            }

            public object Key
            {
                get { return this.GetType(); }
            }

            public _Int32 Column { get; }
        }

        private sealed class SysParentRowId : IAddon
        {
            public SysParentRowId(_Int32 column)
            {
                Column = column;
            }

            public object Key
            {
                get { return this.GetType(); }
            }

            public _Int32 Column { get; }
        }

        internal _Int32 GetSysRowIdColumn(bool createIfNotExist)
        {
            var sysRowId = GetAddon<SysRowId>();
            if (sysRowId == null)
            {
                if (!createIfNotExist)
                    return null;

                sysRowId = new SysRowId(AddSysRowIdColumn(false));
                AddOrUpdate(sysRowId);
            }
            return sysRowId.Column;
        }

        internal _Int32 GetSysParentRowIdColumn(bool createIfNotExist)
        {
            var sysParentRowId = GetAddon<SysParentRowId>();
            if (sysParentRowId == null)
            {
                if (!createIfNotExist)
                    return null;

                sysParentRowId = new SysParentRowId(AddSysRowIdColumn(true));
                AddOrUpdate(sysParentRowId);
            }
            return sysParentRowId.Column;
        }

        /// <summary>
        /// Adds database index.
        /// </summary>
        /// <param name="name">The name of the index.</param>
        /// <param name="description">The description of the index.</param>
        /// <param name="isUnique">Specifies whether this index is unique.</param>
        /// <param name="isClustered">Specifies whether this index is clustered.</param>
        /// <param name="isValidOnTable">Specifies whether this index is for permanent table.</param>
        /// <param name="isValidOnTempTable">Specifies whether this index is for tempoary table.</param>
        /// <param name="orderByList">The columns and sorting order to make up this index.</param>
        protected internal void AddDbIndex(string name, string description, bool isUnique, bool isClustered, bool isValidOnTable, bool isValidOnTempTable, params ColumnSort[] orderByList)
        {
            name.VerifyNotEmpty(nameof(name));
            orderByList.VerifyNotNull(nameof(orderByList));
            if (orderByList.Length == 0)
                throw new ArgumentException(DiagnosticMessages.Model_EmptyColumns, nameof(orderByList));

            for (int i = 0; i < orderByList.Length; i++)
            {
                var column = orderByList[i].Column;
                if (column == null || column.ParentModel != this)
                    throw new ArgumentException(DiagnosticMessages.Model_VerifyChildColumn, string.Format(CultureInfo.InvariantCulture, nameof(orderByList) + "[{0}]", i));
            }

            AddDbIndex(new DbIndex(name, description, isUnique, isClustered, isValidOnTable, isValidOnTempTable, orderByList));
        }

        /// <summary>
        /// Adds database unique constraint.
        /// </summary>
        /// <param name="name">The name of the constraint.</param>
        /// <param name="description">The description of the constraint.</param>
        /// <param name="isClustered">Specifies whether this unique constraint is clustered.</param>
        /// <param name="orderByList">The columns and sorting order to make up this unique constraint.</param>
        protected internal void AddDbUniqueConstraint(string name, string description, bool isClustered, params ColumnSort[] orderByList)
        {
            orderByList.VerifyNotNull(nameof(orderByList));
            if (orderByList.Length == 0)
                throw new ArgumentException(DiagnosticMessages.Model_EmptyColumns, nameof(orderByList));

            for (int i = 0; i < orderByList.Length; i++)
            {
                var column = orderByList[i].Column;
                if (column == null || column.ParentModel != this)
                    throw new ArgumentException(DiagnosticMessages.Model_VerifyChildColumn, string.Format(CultureInfo.InvariantCulture, nameof(orderByList) + "[{0}]", i));
            }

            AddDbTableConstraint(new DbUniqueConstraint(name, description, isClustered, orderByList), false);
        }

        /// <summary>
        /// Adds database check constraint.
        /// </summary>
        /// <param name="name">The name of the constraint.</param>
        /// <param name="description">The description of the constraint.</param>
        /// <param name="condition">The condition of this check constraint.</param>
        protected internal void AddDbCheckConstraint(string name, string description, _Boolean condition)
        {
            condition.VerifyNotNull(nameof(condition));

            AddDbTableConstraint(new DbCheckConstraint(name, description, condition.DbExpression), false);
        }

        private int _suspendIdentityCount;
        internal bool IsIdentitySuspended
        {
            get { return _suspendIdentityCount > 0; }
        }

        internal void SuspendIdentity()
        {
            _suspendIdentityCount++;
        }

        internal void ResumeIdentity()
        {
            _suspendIdentityCount--;
        }

        internal DbTable<SequentialKey> SequentialKeyTempTable { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.GetType().Name + ": [" + string.Join(", ", Columns.Select(x => x.Name)) + "]";
        }

        internal KeyOutput ParentSequentialKeyModel
        {
            get
            {
                var parentModel = ParentModel;
                if (parentModel == null)
                    return null;

                if (!(parentModel.DataSource is IDbSet parentQuery) || parentQuery.Kind != DataSourceKind.DbQuery)
                    return null;

                var result = parentQuery.QueryStatement.SequentialKeyTempTable._;
                return result;
            }
        }

        internal IReadOnlyList<ColumnMapping> GetParentRelationship(IReadOnlyList<ColumnMapping> columnMappings)
        {
            var parentRelationship = ParentRelationship;
            if (parentRelationship == null)
                return null;

            var result = new ColumnMapping[parentRelationship.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var mapping = parentRelationship[i];
                var source = GetSource(columnMappings, mapping.SourceExpression);
                if (source == null)
                    throw new InvalidOperationException(DiagnosticMessages.Common_ChildColumnNotExistInColumnMappings(mapping.SourceExpression));
                result[i] = new ColumnMapping(source, mapping.Target);
            }

            return result;
        }

        private static DbExpression GetSource(IReadOnlyList<ColumnMapping> mappings, DbExpression target)
        {
            foreach (var mapping in mappings)
            {
                if (mapping.TargetExpression == target)
                    return mapping.SourceExpression;
            }
            return null;
        }

        /// <summary>
        /// Creates a DataSet.
        /// </summary>
        /// <typeparam name="T">Entity type of the DataSet.</typeparam>
        /// <param name="dataSetColumn">The DataSet column.</param>
        /// <param name="rowOrdinal">The ordinal of DataRow.</param>
        /// <returns></returns>
        protected internal virtual DataSet<T> CreateDataSet<T>(_DataSet<T> dataSetColumn, int rowOrdinal)
            where T : Model, new()
        {
            return DataSet<T>.Create();
        }

        internal DataRow EditingRow { get; private set; }

        internal DataRow AddingRow
        {
            get
            {
                var editingRow = EditingRow;
                if (editingRow == null)
                    return null;
                return editingRow.Ordinal < 0 ? editingRow : null;
            }
        }

        internal void BeginEdit(DataRow dataRow)
        {
            Debug.Assert(EditingRow == null && dataRow != null && dataRow.Model == this);
            EnsureInitialized();
            EditingRow = dataRow;
            for (int i = 0; i < Columns.Count; i++)
                Columns[i].BeginEdit(dataRow);
            for (int i = 0; i < LocalColumns.Count; i++)
                LocalColumns[i].BeginEdit(dataRow);
        }

        internal void EndEdit(bool discardChanges)
        {
            Debug.Assert(EditingRow != null);

            var editingRow = EditingRow;

            editingRow.SuspendValueChangedNotification(discardChanges);
            for (int i = 0; i < Columns.Count; i++)
                Columns[i].EndEdit(editingRow);
            for (int i = 0; i < LocalColumns.Count; i++)
                LocalColumns[i].EndEdit(editingRow);
            EditingRow = null;
            editingRow.ResumeValueChangedNotification();
        }

        internal void CancelEdit()
        {
            Debug.Assert(EditingRow != null);
            EditingRow = null;
        }

        internal void LockEditingRow(Action action)
        {
            var editingRow = EditingRow;
            EditingRow = null;
            action();
            EditingRow = editingRow;
        }

        internal Column DeserializeColumn(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return null;

            var result = Columns[columnName];
            if (result == null)
                throw new FormatException(DiagnosticMessages.Model_InvalidColumnName(columnName));
            return result;
        }

        /// <summary>
        /// Occurs when this model is constructing.
        /// </summary>
        public event EventHandler<EventArgs> Constructing = delegate { };

        /// <summary>
        /// Occurs when this model is initializing.
        /// </summary>
        public event EventHandler<EventArgs> Initializing = delegate { };

        /// <summary>
        /// Occurs when child models are mounted.
        /// </summary>
        public event EventHandler<EventArgs> ChildModelsMounted = delegate { };

        /// <summary>
        /// Occurs when child DataSets are created.
        /// </summary>
        public event EventHandler<EventArgs> ChildDataSetsCreated = delegate { };

        /// <summary>
        /// Occurs when this model is initialized.
        /// </summary>
        public event EventHandler<EventArgs> Initialized = delegate { };

        /// <summary>
        /// Occurs when inserting DataRow.
        /// </summary>
        public event EventHandler<DataRowEventArgs> DataRowInserting = delegate { };

        /// <summary>
        /// Occurs before DataRow inserted.
        /// </summary>
        public event EventHandler<DataRowEventArgs> BeforeDataRowInserted = delegate { };

        /// <summary>
        /// Occurs after DataRow inserted.
        /// </summary>
        public event EventHandler<DataRowEventArgs> AfterDataRowInserted = delegate { };

        /// <summary>
        /// Occurs when removing DataRow.
        /// </summary>
        public event EventHandler<DataRowEventArgs> DataRowRemoving = delegate { };

        /// <summary>
        /// Occurs when DataRow is removed.
        /// </summary>
        public event EventHandler<DataRowRemovedEventArgs> DataRowRemoved = delegate { };

        /// <summary>
        /// Ocurrs when data value is changed.
        /// </summary>
        public event EventHandler<ValueChangedEventArgs> ValueChanged = delegate { };

        internal void HandlesDataRowInserted(DataRow dataRow, Action<DataRow> updateAction)
        {
            dataRow.SuspendValueChangedNotification(true);
            var e = new DataRowEventArgs(dataRow);
            OnDataRowInserting(e);
            DataSetContainer.OnDataRowInserting(e);
            updateAction?.Invoke(dataRow);
            OnBeforeDataRowInserted(e);
            DataSetContainer.OnBeforeDataRowInserted(e);
            dataRow.ResumeValueChangedNotification();
            DataSetContainer.SuspendComputation();
            OnAfterDataRowInserted(e);
            DataSetContainer.OnAfterDataRowInserted(e);
            DataSetContainer.ResumeComputation();
        }

        /// <summary>
        /// Raises <see cref="DataRowInserting"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataRowInserting(DataRowEventArgs e)
        {
            DataRowInserting(this, e);
        }

        /// <summary>
        /// Raises <see cref="BeforeDataRowInserted"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnBeforeDataRowInserted(DataRowEventArgs e)
        {
            BeforeDataRowInserted(this, e);
        }

        /// <summary>
        /// Raises <see cref="AfterDataRowInserted"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnAfterDataRowInserted(DataRowEventArgs e)
        {
            AfterDataRowInserted(this, e);
        }

        internal void HandlesDataRowRemoving(DataRow dataRow)
        {
            dataRow.SuspendValueChangedNotification(true);
            var e = new DataRowEventArgs(dataRow);
            OnDataRowRemoving(e);
            DataSetContainer.OnDataRowRemoving(e);
        }

        /// <summary>
        /// Raises <see cref="DataRowRemoving"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataRowRemoving(DataRowEventArgs e)
        {
            DataRowRemoving(this, e);
        }

        internal void HandlesDataRowRemoved(DataRow dataRow, DataSet baseDataSet, int ordinal, DataSet dataSet, int index)
        {
            if (EditingRow == dataRow)
                CancelEdit();

            dataRow.ResumeValueChangedNotification();
            DataSetContainer.SuspendComputation();
            var e = new DataRowRemovedEventArgs(dataRow, baseDataSet, ordinal, dataSet, index);
            OnDataRowRemoved(e);
            DataSetContainer.OnDataRowRemoved(e);
            DataSetContainer.ResumeComputation();
        }

        /// <summary>
        /// Raises <see cref="DataRowRemoved"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataRowRemoved(DataRowRemovedEventArgs e)
        {
            DataRowRemoved(this, e);
        }

        internal void HandlesValueChanged(DataRow dataRow, IColumns columns)
        {
            var e = new ValueChangedEventArgs(dataRow, columns);
            if (dataRow.IsEditing)
            {
                OnValueChanged(e);
                return;
            }

            DataSetContainer.SuspendComputation();
            OnValueChanged(e);
            DataSetContainer.OnValueChanged(e);
            DataSetContainer.ResumeComputation();
        }

        /// <summary>
        /// Raises <see cref="ValueChanged"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnValueChanged(ValueChangedEventArgs e)
        {
            ValueChanged(this, e);
        }

        private DataSetContainer _dataSetContainer;
        /// <summary>
        /// Gets the DataSet container which owns this model.
        /// </summary>
        public DataSetContainer DataSetContainer
        {
            get { return RootModel._dataSetContainer; }
        }

        private int _suspendEditingValueCount = 0;
        internal void SuspendEditingValue()
        {
            RootModel._suspendEditingValueCount++;
        }

        internal void ResumeEditingValue()
        {
            Debug.Assert(IsEditingValueSuspended);
            RootModel._suspendEditingValueCount--;
        }

        internal bool IsEditingValueSuspended
        {
            get { return RootModel._suspendEditingValueCount > 0; }
        }

        /// <summary>
        /// Gets the name of this model.
        /// </summary>
        /// <returns>The name of this model.</returns>
        public string GetName()
        {
            return Name;
        }

        /// <summary>
        /// Gets the columns created by this model.
        /// </summary>
        /// <returns>The columns created by this model.</returns>
        public ColumnCollection GetColumns()
        {
            return Columns;
        }

        /// <summary>
        /// Gets the local columns owned by this model.
        /// </summary>
        /// <returns>The local columns owned by this model.</returns>
        public IReadOnlyList<Column> GetLocalColumns()
        {
            return LocalColumns;
        }

        /// <summary>
        /// Gets the column lists owned by this model.
        /// </summary>
        /// <returns>The column lists owned by this model.</returns>
        public IReadOnlyList<ColumnList> GetColumnLists()
        {
            return ColumnLists;
        }

        /// <summary>
        /// Gets the ordinal of this child model.
        /// </summary>
        /// <returns>The ordinal of this child model.</returns>
        public int GetOrdinal()
        {
            return Ordinal;
        }

        /// <summary>
        /// Gets the DataSource which owns this model.
        /// </summary>
        /// <returns>The DataSource which owns this model.</returns>
        public DataSource GetDataSource()
        {
            return DataSource;
        }

        /// <summary>
        /// Gets the DataSet which owns this model.
        /// </summary>
        /// <returns>The DataSet which owns this model.</returns>
        public DataSet GetDataSet()
        {
            return DataSet;
        }

        /// <summary>
        /// Gets the depth of this nested model.
        /// </summary>
        /// <returns>The depth of this nested model.</returns>
        public int GetDepth()
        {
            return Depth;
        }

        internal void Insert(DataRow dataRow)
        {
            foreach (var column in Columns)
                column.InsertRow(dataRow);

            foreach (var column in LocalColumns)
                column.InsertRow(dataRow);

            foreach (var projection in Projections)
                projection.Insert(dataRow);
        }

        internal void Remove(DataRow dataRow)
        {
            foreach (var column in Columns)
                column.RemoveRow(dataRow);

            foreach (var column in LocalColumns)
                column.RemoveRow(dataRow);

            foreach (var projection in Projections)
                projection.Remove(dataRow);
        }

        /// <summary>
        /// Gets the total column count, including recursive projection(s).
        /// </summary>
        internal int TotalColumnCount
        {
            get
            {
                var result = Columns.Count;
                for (int i = 0; i < Projections.Count; i++)
                    result += Projections[i].TotalColumnCount;
                return result;
            }
        }

        internal int ColumnOrdinalOffset { get; private set; }

        private ColumnCollection _allColumns;
        internal ColumnCollection AllColumns
        {
            get
            {
                if (Projections.Count == 0)
                    return Columns;

                return _allColumns ?? (_allColumns = GetAllColumns());
            }
        }

        private ColumnCollection GetAllColumns()
        {
            var result = new ColumnCollection(this);
            AddAllColumns(result);
            return result;
        }

        private void AddAllColumns(ColumnCollection result)
        {
            var columns = Columns;
            for (int i = 0; i < columns.Count - columns.SystemColumnCount; i++) // make sure system column(s) at tail
                AddToAllColumns(result, columns[i]);

            foreach (var projection in Projections)
                projection.AddAllColumns(result);

            for (int i = columns.Count - columns.SystemColumnCount; i < columns.Count; i++) // add system column(s)
                AddToAllColumns(result, columns[i]);
        }

        private static void AddToAllColumns(ColumnCollection columns, Column item)
        {
            if (item.Ordinal != columns.Count)
                Debug.Assert(item.Ordinal == columns.Count);
            columns.Add(item);
        }
    }
}
