using Microsoft.Extensions.Logging;
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

        public IEnumerable<KeyValuePair<string, SoftwareSourceInfo>> GetSources(SoftwareProductInfo product)
        {
            return Data
                .Where(kvp => IsMatch(kvp.Value, product));
        }

        public IEnumerable<SoftwareSourceInfo> GetSources(SoftwareProductInfo product, string sourceName)
        {
            return Data
                .Select(kvp => kvp.Value)
                .Where(s => IsMatch(s, product, sourceName));
        }

        protected abstract string CategoryName { get; }

        protected abstract string ProductName { get; }

        protected virtual string GetChannelName(SoftwareProductInfo product) => null;

        protected virtual CultureInfo GetLanguage(SoftwareSourceInfo source) => null;

        private bool IsMatch(string categoryName)
        {
            if (categoryName == null)
                return true;

            return CategoryName.Equals(categoryName, StringComparison.InvariantCulture);
        }

        private bool IsMatch(SoftwareProductInfo product)
        {
            if (!IsMatch(product?.Category))
                return false;

            if (product?.Name == null)
                return true;

            return ProductName.Equals(product.Name, StringComparison.InvariantCulture);
        }

        private bool IsMatch(SoftwareSourceInfo source, SoftwareProductInfo product)
        {
            if (!IsMatch(product))
                return false;

            var channelName = GetChannelName(product);
            if (channelName == null)
                return true;

            if (!channelName.Equals(source.Channel, StringComparison.InvariantCulture))
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
                && sourceName.Equals(source.Name, StringComparison.InvariantCulture);
        }
    }
}
