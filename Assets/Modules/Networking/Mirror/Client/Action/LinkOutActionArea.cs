using Mirror;
using Zenject;
using UnityEngine;
using System.Collections;
using com.playbux.action;
using com.playbux.ui;
using com.playbux.ui.gamemenu;

namespace com.playbux.networking.mirror.client.action
{
    public class LinkOutActionArea : ClientActionArea
    {
        [SerializeField]
        private DialogLinkOutData dialogLinkOutData;

        private DialogTempleteController dialogTempleteController;

        [Inject]
        private void Construct( DialogTempleteController dialogTempleteController)
        {
           this.dialogTempleteController = dialogTempleteController;
        }

        public override void Dispose()
        {
            dialogTempleteController.OnClose -= OnClose;
            base.Dispose();
        }

        public override void OnAreaEnter()
        {
            dialogTempleteController.OnClose += OnClose;
            dialogTempleteController.SetData(dialogLinkOutData);
            dialogTempleteController.OpenDialogTemplate();
        }

        private void OnClose()
        {
            dialogTempleteController.OnClose -= OnClose;
        }
    }
}