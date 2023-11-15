using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HamerSoft.BetterResources
{
    /// <summary>
    /// A Query Builder to Search the Unity3D Resources
    /// <remarks>This requires a Cache is loaded during <see cref="BetterResources"/>.Initialize | InitializeAsync | InitializeRoutine.</remarks>
    /// <remarks> QueryBuilder implements IDisposable, so use a using statement!</remarks>
    /// <remarks> You can no longer change a query after you call <see cref="GetResult"/> | <see cref="GetResult{T}"/> | <see cref="GetResults"/> | <see cref="GetResults{T}"/>.</remarks>
    /// </summary>
    public class QueryBuilder : IDisposable
    {
        private ResourceManifest _manifest;
        private List<Func<ResourceInfo, bool>> _queryPredicate;
        private Func<ResourceInfo, bool> _namePredicate;
        private Func<ResourceInfo, bool> _pathPredicate;
        private Func<ResourceInfo, bool> _packagePredicate;
        private Func<ResourceInfo, bool> _typePredicate;
        private Func<ResourceInfo, bool> _inPackagePredicate;
        private Func<ResourceInfo, bool> _withComponentsPredicate;
        private Func<ResourceInfo, bool> _withoutComponentsPredicate;
        private Func<ResourceInfo, bool> _guidPredicate;
        private IEnumerable<ResourceInfo> _results;
        private bool _isDisposed;

        internal QueryBuilder(ResourceManifest manifest)
        {
            _manifest = manifest;
            _queryPredicate = new List<Func<ResourceInfo, bool>>(5);
        }

        /// <summary>
        /// Get a single <see cref="ResourceInfo"/> result that has a component that matches type T or is derived from T and match the other filters
        /// </summary>
        /// <typeparam name="T">Generic type filter</typeparam>
        /// <remarks> You can no longer change a query after you call <see cref="GetResult"/> | <see cref="GetResult{T}"/> | <see cref="GetResults"/> | <see cref="GetResults{T}"/>.</remarks>
        /// <returns>A <see cref="ResourceInfo"/> that matches all filter and type filter T</returns>
        public ResourceInfo GetResult<T>() where T : UnityEngine.Object
        {
            return GetResults<T>()?.FirstOrDefault();
        }
        
        /// <summary>
        /// Get <see cref="ResourceInfo"/> results that have a component that matches type T or is derived from T and match the other filters
        /// </summary>
        /// <typeparam name="T">Generic type filter</typeparam>
        /// <remarks> You can no longer change a query after you call <see cref="GetResult"/> | <see cref="GetResult{T}"/> | <see cref="GetResults"/> | <see cref="GetResults{T}"/>.</remarks>
        /// <returns>A collection of <see cref="ResourceInfo"/> that match all filter and type filter T</returns>
        public IEnumerable<ResourceInfo> GetResults<T>() where T : UnityEngine.Object
        {
            if (_isDisposed)
                return null;
            if (_results != null)
                return _results;
            _queryPredicate.Remove(_typePredicate);
            if (typeof(T) != typeof(UnityEngine.Object))
            {
                _typePredicate = (resource) =>
                    resource.Components.Any(c => c == typeof(T) || c.IsSubclassOf(typeof(T)));
                _queryPredicate.Add(_typePredicate);
            }

            Func<ResourceInfo, bool> predicate = _guidPredicate ?? ((resource) =>
            {
                return _queryPredicate.All(pred => pred.Invoke(resource));
            });

            _results = _manifest?.Resources.Where(resource => predicate.Invoke(resource)) ??
                       Array.Empty<ResourceInfo>();
            return _results;
        }
        
        /// <summary>
        /// Get a single <see cref="ResourceInfo"/> result that match the filters
        /// </summary>
        /// <remarks> You can no longer change a query after you call <see cref="GetResult"/> | <see cref="GetResult{T}"/> | <see cref="GetResults"/> | <see cref="GetResults{T}"/>.</remarks>
        /// <returns>A <see cref="ResourceInfo"/> that matches all filters</returns>
        public ResourceInfo GetResult()
        {
            if (_queryPredicate?.All(p => p == null) == true)
            {
                _results = Array.Empty<ResourceInfo>();
                return null;
            }

            return GetResult<UnityEngine.Object>();
        }

        /// <summary>
        /// Get <see cref="ResourceInfo"/> results that match the filters
        /// </summary>
        /// <remarks> You can no longer change a query after you call <see cref="GetResult"/> | <see cref="GetResult{T}"/> | <see cref="GetResults"/> | <see cref="GetResults{T}"/>.</remarks>
        /// <returns>A collection of <see cref="ResourceInfo"/> that match all filters</returns>
        public IEnumerable<ResourceInfo> GetResults()
        {
            return GetResults<UnityEngine.Object>();
        }

        /// <summary>
        /// Add a name filter
        /// </summary>
        /// <param name="nameFilter">Exact name of the object of interest</param>
        /// <param name="comparison">Comparison culture for exact filtering</param>
        /// <remarks> Default comparison is CurrentCulture</remarks>
        /// <remarks> Setting a name filter cancels out the <see cref="ByNameSubString"/> and vice-versa</remarks>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with Name Filter added.</returns>
        public QueryBuilder ByName(string nameFilter, StringComparison comparison = StringComparison.CurrentCulture)
        {
            SetPredicate(nameFilter, ref _namePredicate,
                (namedResource) => namedResource.Name.Equals(nameFilter, comparison));
            return this;
        }
        
        /// <summary>
        /// Add a GUID filter
        /// </summary>
        /// <param name="guidFilter">String GUID as given by the <see cref="UnityEditor.AssetDatabase"/></param>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with GUID Filter added.</returns>
        public QueryBuilder ByGuid(string guidFilter)
        {
            SetPredicate(guidFilter, ref _guidPredicate,
                (guidResource) => guidResource.Guid.Equals(guidFilter));
            return this;
        }

        /// <summary>
        /// Add a GUID filter
        /// </summary>
        /// <param name="guid">GUID object as given by the <see cref="UnityEditor.AssetDatabase"/></param>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with GUID Filter added.</returns>
        public QueryBuilder ByGuid(Guid guid)
        {
            SetPredicate(guid.ToString(), ref _guidPredicate,
                (guidResource) => Guid.TryParse(guidResource.Guid, out var g) && g == guid);
            return this;
        }

        /// <summary>
        /// Add a name substring filter
        /// </summary>
        /// <param name="nameFilter">Substring of the name of the object of interest.</param>
        /// <param name="comparison">Comparison culture for exact filtering.</param>
        /// <remarks> Default comparison is CurrentCulture.</remarks>
        /// <remarks> Setting a substring name filter cancels out the <see cref="ByName"/> and vice-versa.</remarks>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with NameSubString Filter added.</returns>
        public QueryBuilder ByNameSubString(string nameFilter,
            StringComparison comparison = StringComparison.CurrentCulture)
        {
            SetPredicate(nameFilter, ref _namePredicate,
                (namedResource) => namedResource.Name.Contains(nameFilter, comparison));
            return this;
        }

        /// <summary>
        /// Add a filter to only find assets that are in the Root Resources folders
        /// </summary>
        /// <remarks> Any Resources folder is seen as root, also when Resource folders are nested!</remarks>
        /// <remarks> Setting a root filter cancels out the <see cref="ByPath"/>, <see cref="ByPathSubString"/> and vice-versa.</remarks>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with root Filter added.</returns>
        public QueryBuilder AtRoot()
        {
            SetPredicate("", ref _pathPredicate,
                (pathResource) => Path.GetDirectoryName(pathResource.ResourcesPath) == "", true);
            return this;
        }

        /// <summary>
        /// Add a filter for an exact directory
        /// </summary>
        /// <param name="pathFilter">Directory local to resources</param>
        /// <param name="comparison">Comparison culture for exact filtering</param>
        /// <remarks> Default comparison is CurrentCulture</remarks>
        /// <remarks> Setting a path filter cancels out the <see cref="AtRoot"/>, <see cref="ByPathSubString"/> and vice-versa.</remarks>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with path Filter added.</returns>
        public QueryBuilder ByPath(string pathFilter, StringComparison comparison = StringComparison.CurrentCulture)
        {
            SetPredicate(pathFilter, ref _pathPredicate,
                (pathResource) => pathResource.ResourcesPath.StartsWith(pathFilter, comparison));
            return this;
        }

        /// <summary>
        /// Add a path substring filter
        /// </summary>
        /// <param name="pathFilter">Substring of the path of the object(s) of interest.</param>
        /// <param name="comparison">Comparison culture for exact filtering.</param>
        /// <remarks> Default comparison is CurrentCulture.</remarks>
        /// <remarks> Setting a substring name filter cancels out the <see cref="ByPath"/>, <see cref="AtRoot"/> and vice-versa.</remarks>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with PathSubString Filter added.</returns>
        public QueryBuilder ByPathSubString(string pathFilter,
            StringComparison comparison = StringComparison.CurrentCulture)
        {
            SetPredicate(pathFilter, ref _pathPredicate,
                (pathResource) => pathResource.ResourcesPath.Contains(pathFilter, comparison));
            return this;
        }

        /// <summary>
        /// Add a filter for an exact package name e.g. com.hamersoft.betterresources
        /// </summary>
        /// <param name="packageFilter">Package name loaded through the Unity3D PackageManager</param>
        /// <param name="comparison">Comparison culture for exact filtering</param>
        /// <remarks> Default comparison is CurrentCulture</remarks>
        /// <remarks> Setting a package filter cancels out the <see cref="ByPackageSubString"/> and vice-versa.</remarks>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with package Filter added.</returns>
        public QueryBuilder ByPackage(string packageFilter,
            StringComparison comparison = StringComparison.CurrentCulture)
        {
            if (!string.IsNullOrWhiteSpace(packageFilter))
                RemovePredicate(ref _inPackagePredicate);

            SetPredicate(packageFilter, ref _packagePredicate,
                (pathResource) => pathResource.IsInPackage && pathResource.Package.Equals(packageFilter, comparison));
            return this;
        }

        /// <summary>
        /// Add a filter for a substring in a package name
        /// </summary>
        /// <param name="packageFilter">Package name substring loaded through the Unity3D PackageManager</param>
        /// <param name="comparison">Comparison culture for exact filtering</param>
        /// <remarks> Default comparison is CurrentCulture</remarks>
        /// <remarks> Setting a package substring filter cancels out the <see cref="ByPackage"/> and vice-versa.</remarks>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with package substring Filter added.</returns>
        public QueryBuilder ByPackageSubString(string packageFilter,
            StringComparison comparison = StringComparison.CurrentCulture)
        {
            if (!string.IsNullOrWhiteSpace(packageFilter))
                RemovePredicate(ref _inPackagePredicate);

            SetPredicate(packageFilter, ref _packagePredicate,
                (pathResource) => pathResource.IsInPackage && pathResource.Package.Contains(packageFilter, comparison));
            return this;
        }

        /// <summary>
        /// Add a filter to only find or exclude assets in packages
        /// </summary>
        /// <param name="inPackage">In Package flag</param>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with path Filter added.</returns>
        public QueryBuilder InPackage(bool inPackage)
        {
            SetPredicate("package", ref _inPackagePredicate,
                (pathResource) => pathResource.IsInPackage == inPackage);
            return this;
        }

        /// <summary>
        /// Add a filter where ALL given components must be present on the target object(s)
        /// </summary>
        /// <param name="components">Collection of components to filter for</param>
        /// <remarks>Reminder: Most Prefabs will have a <see cref="UnityEngine.Transform"/> or <see cref="UnityEngine.RectTransform"/>.</remarks>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with AllComponents Filter added.</returns>
        public QueryBuilder WithAllComponents(params Type[] components)
        {
            var set = new HashSet<Type>(components != null ? components.Where(c => c != null) : Array.Empty<Type>());
            if (set.Count == 0)
            {
                RemovePredicate(ref _withComponentsPredicate);
                return this;
            }

            SetPredicate("components", ref _withComponentsPredicate,
                // ReSharper disable once AssignNullToNotNullAttribute
                (pathResource) => set.IsSubsetOf(pathResource.Components));
            return this;
        }

        /// <summary>
        /// Add a filter where some of the given components should be present on the target object(s)
        /// </summary>
        /// <param name="components">Collection of components to filter for</param>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with SomeComponents Filter added.</returns>
        public QueryBuilder WithSomeComponents(params Type[] components)
        {
            var set = new HashSet<Type>(components != null ? components.Where(c => c != null) : Array.Empty<Type>());
            if (set.Count == 0)
            {
                RemovePredicate(ref _withComponentsPredicate);
                return this;
            }

            SetPredicate("components", ref _withComponentsPredicate,
                // ReSharper disable once AssignNullToNotNullAttribute
                (pathResource) => set.Overlaps(pathResource.Components));
            return this;
        }

        /// <summary>
        /// Add a filter where Some of the given components must be present on the target object(s)
        /// </summary>
        /// <param name="components">Collection of components to filter for</param>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with WithoutAnyComponents Filter added.</returns>
        public QueryBuilder WithoutAnyComponents(params Type[] components)
        {
            var set = new HashSet<Type>(components != null ? components.Where(c => c != null) : Array.Empty<Type>());
            if (set.Count == 0)
            {
                RemovePredicate(ref _withoutComponentsPredicate);
                return this;
            }

            SetPredicate("components", ref _withoutComponentsPredicate,
                // ReSharper disable once AssignNullToNotNullAttribute
                (pathResource) => !pathResource.Components.Overlaps(set));
            return this;
        }

        /// <summary>
        /// Add a filter where NONE of the given components must be present on the target object(s)
        /// </summary>
        /// <param name="components">Collection of components to filter for</param>
        /// <returns>The same instance of <see cref="QueryBuilder"/> but with WithoutAllComponents Filter added.</returns>
        public QueryBuilder WithoutAllComponents(params Type[] components)
        {
            var set = new HashSet<Type>(components != null ? components.Where(c => c != null) : Array.Empty<Type>());
            if (set.Count == 0)
            {
                RemovePredicate(ref _withoutComponentsPredicate);
                return this;
            }
            
            SetPredicate("components", ref _withoutComponentsPredicate,
                // ReSharper disable once AssignNullToNotNullAttribute
                (pathResource) => !pathResource.Components.IsSupersetOf(set));
            return this;
        }

        private void SetPredicate(string filter, ref Func<ResourceInfo, bool> reference,
            Func<ResourceInfo, bool> predicate, bool ignoreNull = false)
        {
            if (string.IsNullOrWhiteSpace(filter) && !ignoreNull)
            {
                _queryPredicate.Remove(reference);
                reference = null;
            }
            else
            {
                if (reference != null)
                    _queryPredicate.Remove(reference);

                reference = predicate;
                _queryPredicate.Add(reference);
            }
        }

        private void RemovePredicate(ref Func<ResourceInfo, bool> predicate)
        {
            if (predicate != null)
                _queryPredicate.Remove(predicate);
            predicate = null;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            _manifest = null;
            _queryPredicate = null;
            _namePredicate = null;
            _pathPredicate = null;
            _packagePredicate = null;
            _typePredicate = null;
            _inPackagePredicate = null;
            _withComponentsPredicate = null;
            _withoutComponentsPredicate = null;
            _guidPredicate = null;
            _results = null;
        }
    }
}