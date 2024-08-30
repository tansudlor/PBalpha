using System;
using UnityEngine;
using UnityEngine.UI;
using com.playbux.sfxwrapper;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using com.playbux.networking.networkavatar;

namespace com.playbux.networking.networkinventory
{
    public sealed class CellButton : MonoBehaviour, IPointerClickHandler, IPointerMoveHandler, IPointerDownHandler, IPointerExitHandler
    {
        public event Action OnButtonClicked;

        public bool equiped;
        public Image Effected;
        public Image Selector;
        public InfoDialog InfoDialog;
        public NftWithFullData Fulldata;
        public NetworkAvatarBoard Board;
        public InventoryUIController Controller;
        public Dictionary<string, string> CollectionToSuffix;

        private void OnDestroy()
        {
            OnButtonClicked = null;
        }

        private void ButtonClicked(CellButton cell)
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            if (!equiped)
            {

                Controller.dialogInfo.SetActive(true);
                InfoDialog.SetInfo("Equip", cell, CollectionToSuffix, cell.Fulldata.Nft.Name, cell.Fulldata.Nft.Rarity, cell.Fulldata.Nft.Description, cell.gameObject, cell.Fulldata.Nft.Collection, cell.Fulldata.RarityIndex);
            }
            else
            {
                Controller.dialogInfo.SetActive(true);
                InfoDialog.SetInfo("Unequip", cell, CollectionToSuffix, cell.Fulldata.Nft.Name, cell.Fulldata.Nft.Rarity, cell.Fulldata.Nft.Description, cell.gameObject, cell.Fulldata.Nft.Collection, cell.Fulldata.RarityIndex);
            }
        }

        private void ButtonRightClicked(CellButton cell)
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            if (!equiped)
            {
                Controller.dialogInfo.SetActive(true);
                InfoDialog.SetInfo("Equip", cell, CollectionToSuffix, cell.Fulldata.Nft.Name, cell.Fulldata.Nft.Rarity, cell.Fulldata.Nft.Description, cell.gameObject, cell.Fulldata.Nft.Collection, cell.Fulldata.RarityIndex);
                InfoDialog.EquipItem();
            }
            else
            {
                Controller.dialogInfo.SetActive(true);
                InfoDialog.SetInfo("Unequip", cell, CollectionToSuffix, cell.Fulldata.Nft.Name, cell.Fulldata.Nft.Rarity, cell.Fulldata.Nft.Description, cell.gameObject, cell.Fulldata.Nft.Collection, cell.Fulldata.RarityIndex);
                InfoDialog.EquipItem();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnButtonClicked?.Invoke();
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            if (eventData.button == PointerEventData.InputButton.Right)
                ButtonRightClicked(this);
            else
                ButtonClicked(this);

            Effected.CrossFadeAlpha(0.75f, 0.01f, true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Effected.CrossFadeAlpha(0.3f, 0.05f, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Effected.CrossFadeAlpha(1f, 0.5f, true);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            Effected.CrossFadeAlpha(0.5f, 0.05f, true);
        }
    }
}