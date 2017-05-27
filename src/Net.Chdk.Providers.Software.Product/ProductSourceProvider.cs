using Net.Chdk.Model.Software;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Net.Chdk.Providers.Software.Product
{
    public abstract class ProductSourceProvider : IProductSourceProvider
    {
        public ProductSourceProvider()
        {
            sources = new Lazy<IDictionary<string, SoftwareSourceInfo>>(GetSources);
        }

        #region Sources

        private const string DataPath = "Data";
        private const string SourcesFileName = "sources.json";

        private readonly Lazy<IDictionary<string, SoftwareSourceInfo>> sources;

        private IDictionary<string, SoftwareSourceInfo> Sources => sources.Value;

        private IDictionary<string, SoftwareSourceInfo> GetSources()
        {
            var filePath = Path.Combine(DataPath, ProductName, SourcesFileName);
            using (var reader = File.OpenText(filePath))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return JsonSerializer.CreateDefault().Deserialize<IDictionary<string, SoftwareSourceInfo>>(jsonReader);
            }
        }

        #endregion

        public IEnumerable<KeyValuePair<string, SoftwareSourceInfo>> GetSources(SoftwareProductInfo product)
        {
            return Sources
                .Where(kvp => IsMatch(kvp.Value, product));
        }

        public IEnumerable<SoftwareSourceInfo> GetSources(SoftwareProductInfo product, string sourceName)
        {
            return Sources
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
