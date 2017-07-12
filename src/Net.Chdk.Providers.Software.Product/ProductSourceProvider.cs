using Microsoft.Extensions.Logging;
using Net.Chdk.Model.Category;
using Net.Chdk.Model.Software;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Net.Chdk.Providers.Software.Product
{
    public abstract class ProductSourceProvider : DataProvider<Dictionary<string, SoftwareSourceInfo>>, IProductSourceProvider
    {
        #region Constants

        private const string DataFileName = "sources.json";

        #endregion

        #region Constructor

        protected ProductSourceProvider(ILogger logger)
            : base(logger)
        {
        }

        #endregion

        #region Data

        protected override string GetFilePath()
        {
            return Path.Combine(Directories.Data, Directories.Product, ProductName, DataFileName);
        }

        #endregion

        public IEnumerable<ProductSource> GetSources(CategoryInfo category)
        {
            return Data
                .Where(kvp => IsMatch(kvp.Value, category))
                .Select(CreateProductSource);
        }

        public IEnumerable<ProductSource> GetSources(SoftwareProductInfo product)
        {
            return Data
                .Where(kvp => IsMatch(kvp.Value, product))
                .Select(CreateProductSource);
        }

        public IEnumerable<SoftwareSourceInfo> GetSources(SoftwareProductInfo product, string sourceName)
        {
            return Data
                .Select(kvp => kvp.Value)
                .Where(s => IsMatch(s, product, sourceName));
        }

        public abstract string ProductName { get; }

        protected abstract string CategoryName { get; }

        protected virtual string GetChannelName(SoftwareProductInfo product) => null;

        protected virtual CultureInfo GetLanguage(SoftwareSourceInfo source) => null;

        private bool IsMatch(SoftwareSourceInfo source, CategoryInfo category)
        {
            if (category?.Name == null)
                return true;

            return CategoryName.Equals(category.Name, StringComparison.Ordinal);
        }

        private bool IsMatch(SoftwareProductInfo product)
        {
            if (product?.Name == null)
                return true;

            return ProductName.Equals(product.Name, StringComparison.Ordinal);
        }

        private bool IsMatch(SoftwareSourceInfo source, SoftwareProductInfo product)
        {
            if (!IsMatch(product))
                return false;

            var channelName = GetChannelName(product);
            if (channelName == null)
                return true;

            if (!channelName.Equals(source.Channel, StringComparison.Ordinal))
                return false;

            if (product.Language == null)
                return true;

            var language = GetLanguage(source);
            if (language == null)
                return true;

            return language.Equals(product.Language);
        }

        private bool IsMatch(SoftwareSourceInfo source, SoftwareProductInfo product, string sourceName)
        {
            return IsMatch(source, product)
                && sourceName.Equals(source.Name, StringComparison.Ordinal);
        }

        private ProductSource CreateProductSource(KeyValuePair<string, SoftwareSourceInfo> kvp)
        {
            return new ProductSource(ProductName, kvp.Key, kvp.Value);
        }
    }
}
