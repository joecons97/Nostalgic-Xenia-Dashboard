using Cysharp.Threading.Tasks;
using LibraryPlugin;
using QRCoder;
using QRCoder.Unity;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace SteamLibraryPlugin
{
    public class SteamLibraryPlugin : LibraryPlugin.LibraryPlugin
    {
        public override string Name => "Steam";

        public override string Description => "Steam";

        public override string IconPath => "steam.png";

        private ModalService modalService = new();
        private ArtworkService artworkService = new();
        private StartEntryService startEntryService = new();
        private SteamAuthService steamAuthService = new();

        public override async UniTask<ArtworkCollection> GetArtworkCollection(string entryId, CancellationToken cancellationToken)
        {
            var collection = await artworkService.GetArtworkAsync(entryId, cancellationToken);

            return collection;
        }

        public override async UniTask<List<LibraryEntry>> GetEntriesAsync(CancellationToken cancellationToken)
        {
            return await SteamLocalService.GetInstalledGamesAsync(cancellationToken);
        }

        public override UniTask<GameActionResult> TryStartEntryAsync(LibraryEntry entry, CancellationToken cancellationToken)
        {
            startEntryService.Plugin = this;

            return UniTask.FromResult(startEntryService.StartEntry(entry, cancellationToken));
        }

        public override List<LibraryPluginButton> GetButtons()
        {
            var list = new List<LibraryPluginButton>();
            var result = steamAuthService.LoadValidToken();
            if (result == null)
                list.Add(new LibraryPluginButton()
                {
                    Name = "Authenticate",
                    Action = Authenticate
                });
            else
                list.Add(new LibraryPluginButton()
                {
                    Name = "Authenticated: " + result.Username,
                });

            return list;
        }

        private async UniTask Authenticate(CancellationToken cancellationToken)
        {
            Debug.Log("Authenticating");

            var beginResult = await steamAuthService.BeginLoginAsync(cancellationToken);

            var root = new GameObject("Root", typeof(RectTransform), typeof(VerticalLayoutGroup));
            root.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
            root.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            
            var textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(root.transform, false);
            var text = textObj.GetComponent<Text>();
            text.font = Resources.Load<Font>("NXD");
            text.fontSize = 26;
            text.text = "Please can the QR below to authenticate:";

            var qrCodeObj = new GameObject("QRCode", typeof(RectTransform), typeof(RawImage), typeof(LayoutElement), typeof(AspectRatioFitter));
            qrCodeObj.transform.SetParent(root.transform, false);
            var qrCode = qrCodeObj.GetComponent<RawImage>();
            qrCodeObj.GetComponent<AspectRatioFitter>().aspectRatio = 1;
            qrCodeObj.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            qrCodeObj.GetComponent<LayoutElement>().preferredHeight = 256;
            qrCodeObj.GetComponent<LayoutElement>().preferredWidth = 256;

            UpdateQRCodeImage(qrCode, beginResult.Response.ChallengeUrl);

            var id = modalService.CreateModal(new CreateModalArgs()
            {
                Name = "Authenticate",
                CanBeClosed = false,
                ChildrenRoot = root
            });

            Debug.Log(beginResult.Response.ChallengeUrl);

            var token = await steamAuthService.AwaitLoginCompletionAsync(beginResult, (x) => UpdateQRCodeImage(qrCode, x.Response.NewChallengeUrl), cancellationToken);

            Debug.Log("Authenticated!");
            Debug.Log(token.AccessToken);

            steamAuthService.SaveToken(token);

            modalService.CloseModal(id);
        }
        
        private void UpdateQRCodeImage(RawImage imageComponent, string url)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            UnityQRCode qrCode = new UnityQRCode(qrCodeData);
            Texture2D qrCodeAsTexture2D = qrCode.GetGraphic(20);

            imageComponent.texture = qrCodeAsTexture2D;
        }
    }
}
