// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

namespace FruitSwipeMatch3Kit
{
    public class Tile : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private GameObject booster;
        [SerializeField]
        private GameObject horizontalArrows;
        [SerializeField]
        private GameObject verticalArrows;
        [SerializeField]
        private GameObject diagonalLeftArrows;
        [SerializeField]
        private GameObject diagonalRightArrows;
//        [SerializeField]
//        private GameObject linear;
//        [SerializeField]
//        private GameObject horizontalLine;
//        [SerializeField]
//        private GameObject verticalLine;
//        [SerializeField]
//        private GameObject diagonalLeftLine;
//        [SerializeField]
//        private GameObject diagonalRightLine;
#pragma warning restore 649
        
        private void Awake()
        {
            Assert.IsNotNull(booster);
            Assert.IsNotNull(horizontalArrows);
            Assert.IsNotNull(verticalArrows);
            Assert.IsNotNull(diagonalLeftArrows);
            Assert.IsNotNull(diagonalRightArrows);
        }

        private void OnEnable()
        {
            RemoveBooster();
//            RemoveBoosterLine();
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        private void OnDisable()
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
//
//        public bool AddBoosterLine(BoosterType type)
//        {
//            if (type == BoosterType.Normal) return false;
//            if(linear.activeSelf) return false;
//            linear.SetActive(true);
//            horizontalLine.SetActive(false);
//            verticalLine.SetActive(false);
//            diagonalLeftLine.SetActive(false);
//            diagonalRightLine.SetActive(false);
//            switch (type)
//            {
//                case BoosterType.Horizontal:
//                    horizontalLine.SetActive(true);
//                    break;
//                
//                case BoosterType.Vertical:
//                    verticalLine.SetActive(true);
//                    break;
//                
//                case BoosterType.DiagonalLeft:
//                    diagonalLeftLine.SetActive(true);
//                    break;
//                
//                case BoosterType.DiagonalRight:
//                    diagonalRightLine.SetActive(true);
//                    break;
//                
//                case BoosterType.Cross:
//                    horizontalLine.SetActive(true);
//                    verticalLine.SetActive(true);
//                    break;
//                
//                case BoosterType.Star:
//                    horizontalLine.SetActive(true);
//                    verticalLine.SetActive(true);
//                    diagonalLeftLine.SetActive(true);
//                    diagonalRightLine.SetActive(true);
//                    break;
//            }
//
//            return true;
//        }
//        

        public bool AddBooster(BoosterType type)
        {
            if (type == BoosterType.Normal) return false;
            if(booster.activeSelf) return false;
            booster.SetActive(true);
            horizontalArrows.SetActive(false);
            verticalArrows.SetActive(false);
            diagonalLeftArrows.SetActive(false);
            diagonalRightArrows.SetActive(false);
            switch (type)
            {
                case BoosterType.Horizontal:
                    horizontalArrows.SetActive(true);
                    break;
                
                case BoosterType.Vertical:
                    verticalArrows.SetActive(true);
                    break;
                
                case BoosterType.DiagonalLeft:
                    diagonalLeftArrows.SetActive(true);
                    break;
                
                case BoosterType.DiagonalRight:
                    diagonalRightArrows.SetActive(true);
                    break;
                
                case BoosterType.Cross:
                    horizontalArrows.SetActive(true);
                    verticalArrows.SetActive(true);
                    break;
                
                case BoosterType.Star:
                    horizontalArrows.SetActive(true);
                    verticalArrows.SetActive(true);
                    diagonalLeftArrows.SetActive(true);
                    diagonalRightArrows.SetActive(true);
                    break;
            }

            return true;
        }

        public void RemoveBooster()
        {
            booster.SetActive(false);
        }
//
//        public void RemoveBoosterLine()
//        {
//            linear.SetActive(false);
//        }
    }
}
