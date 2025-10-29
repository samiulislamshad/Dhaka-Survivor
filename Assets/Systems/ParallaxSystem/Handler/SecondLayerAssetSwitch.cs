using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.ParallaxSystem.Handler
{
    public class SecondLayerAssetSwitch : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer rend;
        [SerializeField] private List<Sprite> sprites;

        private void Start()
        {
            GetRandomSprite();
        }

        private void GetRandomSprite()
        {
            if(rend == null)
                rend = GetComponent<SpriteRenderer>();
            
            rend.sprite = sprites[Random.Range(0, sprites.Count)];
        }
    }
}