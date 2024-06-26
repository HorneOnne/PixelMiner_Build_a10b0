using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelMiner.Enums;
using System.Linq;
namespace PixelMiner.Core
{
    public class CraftingSystem : MonoBehaviour
    {
        private Dictionary<ItemID, RecipeSO> _recipeBook;
        [SerializeField] private List<RecipeSO> _recipeList;
        private void Awake()
        {
            _recipeBook = new();

            _recipeList = new();
            _recipeList = Resources.LoadAll<RecipeSO>("Recipes").ToList();
        }
    }
}
