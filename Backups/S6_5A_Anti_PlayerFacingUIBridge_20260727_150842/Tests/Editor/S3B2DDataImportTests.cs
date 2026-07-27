using System.Linq;
using NUnit.Framework;
using UnityEngine;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;

namespace GuildMaster.Tests.Editor
{
    public class S3B2DDataImportTests
    {
        [Test]
        public void Validate_DataImport_FromConverter()
        {
            var dataProvider = new EditorExternalGameDataProvider();
            var serializer = new UnityJsonSerializer();
            var database = new GameDatabase();
            var builder = new DatabaseBuilder(dataProvider, serializer, database);
            
            var report = builder.Build();
            
            Assert.IsTrue(report.manifestLoaded, "Manifest failed to load. Check path in EditorExternalGameDataProvider.");
            
            var recipes = database.GetAll<RecipeDefinition>();
            var dungeons = database.GetAll<DungeonDefinition>();
            
            int regOffers = dungeons.Sum(d => d.RegularMerchantOffers?.Count ?? 0);
            int specOffers = dungeons.Sum(d => d.SpecialMerchantOffers?.Count ?? 0);
            
            Debug.Log($"[S3B2D] Recipes loaded: {recipes.Count}");
            Debug.Log($"[S3B2D] Dungeons loaded: {dungeons.Count}");
            Debug.Log($"[S3B2D] Total regular offers loaded: {regOffers}");
            Debug.Log($"[S3B2D] Total special offers loaded: {specOffers}");
            
            if (recipes.Count > 0)
            {
                var firstRecipe = recipes.First();
                Debug.Log($"[S3B2D] First recipe: {firstRecipe.id}, Output: {firstRecipe.OutputItemId}, Ingredients: {firstRecipe.Ingredients?.Count ?? 0}");
            }
            
            if (dungeons.Count > 0)
            {
                var dungeonWithOffers = dungeons.FirstOrDefault(d => d.RegularMerchantOffers != null && d.RegularMerchantOffers.Count > 0);
                if (dungeonWithOffers != null)
                {
                    var offer = dungeonWithOffers.RegularMerchantOffers[0];
                    Debug.Log($"[S3B2D] First merchant offer in {dungeonWithOffers.id}: {offer.ItemId} x{offer.StackCount} w:{offer.Weight}");
                }
            }

            Assert.Greater(recipes.Count, 0, "No recipes loaded.");
            Assert.Greater(dungeons.Count, 0, "No dungeons loaded.");
            Assert.Greater(regOffers, 0, "No regular offers loaded.");
            Assert.Greater(specOffers, 0, "No special offers loaded.");
        }
    }
}
