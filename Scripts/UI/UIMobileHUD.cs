using UnityEngine.UI;
using System.Collections.Generic;
using PixelMiner.Core;
using UnityEngine;
using System;

namespace PixelMiner.UI
{
    public class  UIMobileHUD : CustomCanvas
    {
        private Player _player;
        [SerializeField] private List<UIHeart> _hearts;
        private Transform _heartParent;
  

        private void Start()
        {
         
            _heartParent = transform.Find("HP").transform;
            Main.Instance.OnCharacterInitialize += SetupPlayer;
        }

        private void OnDestroy()
        {

            Main.Instance.OnCharacterInitialize -= SetupPlayer;
        }


        private void SetupPlayer()
        {
            _player = Main.Instance.Players[0].GetComponent<Player>();
            InitializeUIHearts(_player);
        }

        //private void Update()
        //{
        //    if (_player != null)
        //    {
        //        Debug.Log(_player.Health);
        //    }
        //}

        private void InitializeUIHearts(Player player)
        {
            _hearts = new();
            for (int i = 0; i < player.MaxHealth; i++)
            {
                if(i % 10 == 0)
                {
                    var uiheartPrefab = Resources.Load<UIHeart>("UIHeart");
                    if (uiheartPrefab != null)
                    {
                        var uiheartInstance = Instantiate(uiheartPrefab, _heartParent);
                        _hearts.Add(uiheartInstance);
                    }
                }
            }
        }
    }
}
