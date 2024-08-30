
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using com.playbux.events;
using System.Collections;
using System.Collections.Generic;

namespace com.playbux.minimap
{
    public class MiniMapLocator : MonoBehaviour, IScrollHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {

        private static HashSet<string> closeIcon = new HashSet<string>();
        private static HashSet<string> playAnim = new HashSet<string>();
        private static bool refreshIcon = false;
        
        [SerializeField]
        private RawImage miniMap;
        [SerializeField]
        private TextMeshProUGUI realpos;
        [SerializeField]
        private TextMeshProUGUI mappos;
        [SerializeField]
        private MiniMapData master;
        [SerializeField]
        private MiniMapLocator refMiniMapData;
        private MiniMapData data = null;
        [SerializeField]
        private GameObject iconprefab;
        [SerializeField]
        private Transform iconParent;
        [SerializeField]
        private Transform targetPoint;
        [SerializeField]
        private GameObject pingPoint;

        private Vector3 targetSmoothPoint = Vector3.zero;

        private List<GameObject> pingPointList = new List<GameObject>();
        private uint poolSize = 20;
        public IconGroup group;
        private GameObject[] icon;
        private Coroutine pinCoroutine = null;
        private Coroutine pingCoroutine = null;
        private bool isDragging = false;
        public Vector2 MiniMapPoint { get; set; }
        public MiniMapData Data { get => data; set => data = value; }

        public Vector2 FocusPoint
        {
            set
            {
                if (!circleMap)
                {
                    focusPoint = ConvertWorldToMinimap(Data.WorldPoint[0], Data.WorldPoint[1], Data.MapPoint[0], Data.MapPoint[1], value);
                    targetSmoothPoint = focusPoint;


                    targetPoint.gameObject.SetActive(false);


                    if (pinCoroutine != null)
                    {
                        StopCoroutine(pinCoroutine);
                    }
                    pinCoroutine = StartCoroutine(ShowPinPoint());
                }
            }
        }

        public MiniMapLocator RefMiniMapData { set => refMiniMapData = value; }


        public static bool RefreshIconNow { get => refreshIcon; set => refreshIcon = value; }
        public static HashSet<string> CloseIcon { get => closeIcon; set => closeIcon = value; }
        public static HashSet<string> PlayAnim { get => playAnim; set => playAnim = value; }

        [SerializeField]
        private bool circleMap;
        [SerializeField]
        private bool followPlayer;
        private bool isMouseInMap;
        public PointerEventData backupPoint;
        private Vector2 focusPoint;
        private float zoomfinal = 1;

        private float beforeWidth;

        private SignalBus signalBus;

        [Inject]
        void SetUp(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }


        // Start is called before the first frame update
        IEnumerator Start()
        {
            //Debug.Log(pingPoint.name +" pinPoint Mainaaaaaaaaaa"); 
            yield return null;



            if (refMiniMapData != null)
            {
                Debug.Log(refMiniMapData + " refMinimap");
                Debug.Log(refMiniMapData.gameObject.name + " refData");
                yield return new WaitUntil(() => refMiniMapData.Data != null);
                Debug.Log(refMiniMapData + " refMinimap");
                Debug.Log(refMiniMapData.gameObject.name + " refData");
                data = refMiniMapData.Data;
            }
            if (master != null)
            {
                data = Instantiate(master);
            }


            if (data == null)
            {
                Debug.LogError("Minimap Data hasn't been assigned");
            }

            data.hideFlags = HideFlags.HideAndDontSave;

            icon = new GameObject[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                icon[i] = Instantiate(iconprefab, iconParent);
                icon[i].SetActive(true);
               
            }

            var point = new Vector2(miniMap.uvRect.width / 2f, miniMap.uvRect.height / 2f);
            CenterTextureAt(point.x, point.y);
            MiniMapPoint = point;
            focusPoint = point;
            beforeWidth = 1;
            if (!circleMap)
            {
                targetPoint.gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {

            if (Data == null)
            {
                return;
            }

            if (NetworkClient.localPlayer == null)
            {
                return;
            }

            if (NetworkClient.localPlayer.gameObject.transform.position == null)
            {
                return;
            }


            miniMap.texture = Data.Texture;

            var tx = miniMap.uvRect.x;
            var ty = miniMap.uvRect.y;

            realpos.text = "x:" + Mathf.RoundToInt(NetworkClient.localPlayer.gameObject.transform.position.x) + ", y:" + Mathf.RoundToInt(NetworkClient.localPlayer.gameObject.transform.position.y);


            Vector2 worldPointFind = NetworkClient.localPlayer.gameObject.transform.position;




            UpdateIcon();
            //Debug.Log(PinPoint + " pinPoint MaiJAAA");
           
           
            if (!followPlayer)
            {
                // point = new Vector2(miniMap.uvRect.width / 2f, miniMap.uvRect.height / 2f);
                Data.GetIconByName("player")[0].Position = NetworkClient.localPlayer.gameObject.transform.position;
                CenterTextureAt(MiniMapPoint.x, MiniMapPoint.y);
                MiniMapPoint += (focusPoint - MiniMapPoint) / 16;
                var uvRect = miniMap.uvRect;
                uvRect.width += (zoomfinal - uvRect.width) / 16;
                uvRect.height = uvRect.width;
                miniMap.uvRect = uvRect;
                targetPoint.localPosition += (targetSmoothPoint - targetPoint.localPosition) / 64f;

                targetPoint.localScale = (Vector3.one / miniMap.uvRect.width);
                if (targetPoint.localScale.x > 2)
                {
                    targetPoint.localScale = Vector3.one * 2;
                }
                // MiniMapPoint = point;
            }
            else
            {

                var point = ConvertWorldToMinimap(Data.WorldPoint[0], Data.WorldPoint[1], Data.MapPoint[0], Data.MapPoint[1], worldPointFind);
                CenterTextureAt(point.x, point.y);
                mappos.text = point.x + "," + point.y;
                MiniMapPoint = point;
            }


            //miniMap.uvRect = new Rect(point.x, point.y, miniMap.uvRect.width, miniMap.uvRect.height);
        }


        IEnumerator ShowPinPoint()
        {
            yield return new WaitUntil(() => (MiniMapPoint - focusPoint).magnitude < 0.005f);
            targetPoint.gameObject.SetActive(true);
            for (int i = 0; i < 100; i++)
            {
                yield return new WaitForSeconds(0.01f);
                targetPoint.transform.GetChild(0).gameObject.GetComponent<Image>().color = new Color(1, 0, 0, (100f - i) / 100f);
            }
            targetPoint.gameObject.SetActive(false);
        }

        public void CenterTextureAt(float tu, float tv)
        {

            if (miniMap == null)
                return;

            // กำหนดขนาดของ uvRect
            float width = miniMap.uvRect.width;
            float height = miniMap.uvRect.height;

            // คำนวณเพื่อหา offset ใหม่ โดยให้ texture อยู่ตรงกลาง
            float offsetX = tu - (width / 2f);
            float offsetY = tv - (height / 2f);

            // สร้าง uvRect ใหม่ที่จะเลื่อน texture ไปยังตำแหน่งที่ต้องการและมีขนาดตามที่กำหนด
            miniMap.uvRect = new Rect(offsetX, offsetY, width, height);
        }

        private void UpdateIcon()
        {
            if (RefreshIconNow)
            {
                signalBus.Fire(new RefreshIconSignal());
                RefreshIconNow = false;
            }

            

            for (int i = 0; i < poolSize; i++)
            {
                
                if (Data.GetFilterLenght() > i)
                {

                    icon[i].GetComponent<Image>().sprite = Data.GetFilterAt(i).Icon;
                    icon[i].SetActive(true);

                    if (CloseIcon.Contains(Data.GetFilterAt(i).Name))
                    {
                        icon[i].SetActive(false);

                    }

                    if (PlayAnim.Contains(Data.GetFilterAt(i).Name))
                    {
                        icon[i].GetComponent<Animator>().Play("FocusPoint");
                    }

                    var iconMaster = icon[i].GetComponent<IconMasterController>();
                    iconMaster.IconName = Data.GetFilterAt(i).DisplayName;
                    iconMaster.MiniMapLocator = this;
                    iconMaster.SignalBus = signalBus;
                    var point = ConvertWorldToMinimap(Data.WorldPoint[0], Data.WorldPoint[1], Data.MapPoint[0], Data.MapPoint[1], Data.GetFilterAt(i).Position);
                    var delta = (point - MiniMapPoint);
                    var deltaInRect = new Vector2(delta.x * miniMap.rectTransform.sizeDelta.x * 2 * 0.5f / miniMap.uvRect.width * miniMap.transform.localScale.x, delta.y * miniMap.rectTransform.sizeDelta.y * 2 * 0.5f / miniMap.uvRect.height * miniMap.transform.localScale.y);
                    icon[i].transform.localPosition = deltaInRect;


                    if (circleMap)
                    {
                        if (Data.GetFilterAt(i).Name == "player")
                        {
                            icon[i].SetActive(false);
                        }

                        //ทิศทางที่ห่างจากจุดกลาง map
                        var rad = Mathf.Atan2(deltaInRect.y, deltaInRect.x);
                        //ระยะทางจากจุดกลาง map
                        var r = deltaInRect.magnitude;

                        if (r > (miniMap.rectTransform.sizeDelta.x / 2f) - (miniMap.rectTransform.sizeDelta.x / 2f * 15f / 100f))
                        {
                            r = (miniMap.rectTransform.sizeDelta.x / 2f) - (miniMap.rectTransform.sizeDelta.x / 2f * 15f / 100f);
                        }

                        icon[i].transform.localPosition = new Vector3(
                            r * Mathf.Cos(rad),
                            r * Mathf.Sin(rad),
                            0);

                    }
                    else
                    {

                        if (deltaInRect.x > miniMap.rectTransform.sizeDelta.x / 2)
                        {
                            deltaInRect.x = miniMap.rectTransform.sizeDelta.x / 2;
                        }

                        if (deltaInRect.x < -miniMap.rectTransform.sizeDelta.x / 2)
                        {
                            deltaInRect.x = -miniMap.rectTransform.sizeDelta.x / 2;
                        }

                        if (deltaInRect.y > miniMap.rectTransform.sizeDelta.y / 2)
                        {
                            deltaInRect.y = miniMap.rectTransform.sizeDelta.y / 2;
                        }

                        if (deltaInRect.y < -miniMap.rectTransform.sizeDelta.y / 2)
                        {
                            deltaInRect.y = -miniMap.rectTransform.sizeDelta.y / 2;
                        }

                        icon[i].transform.localPosition = deltaInRect;

                        icon[i].transform.localScale = Vector3.one / miniMap.uvRect.width;
                        if (icon[i].transform.localScale.x > 2)
                        {
                            icon[i].transform.localScale = Vector3.one * 2;
                        }




                    }

                }
                else
                {
                    icon[i].SetActive(false);
                }
            }
        }

        public static Vector2 ConvertWorldToMinimap(Vector2 worldPos1, Vector2 worldPos2,
                                                    Vector2 minimapPos1, Vector2 minimapPos2,
                                                    Vector2 targetWorldPos)
        {
            // Calculate transformation coefficients
            float a = (minimapPos1.x - minimapPos2.x) / (worldPos1.x - worldPos2.x);
            float b = minimapPos1.x - a * worldPos1.x;
            float c = (minimapPos1.y - minimapPos2.y) / (worldPos1.y - worldPos2.y);
            float d = minimapPos1.y - c * worldPos1.y;

            // Apply transformation to target world position
            float targetMinimapX = a * targetWorldPos.x + b;
            float targetMinimapY = c * targetWorldPos.y + d;

            return new Vector2(targetMinimapX, targetMinimapY);
        }

        public static Vector2 ConvertMinimapToWorld(Vector2 worldPos1, Vector2 worldPos2,
                                                 Vector2 minimapPos1, Vector2 minimapPos2,
                                                 Vector2 targetMinimapPos)
        {
            // Calculate transformation coefficients
            float a = (minimapPos1.x - minimapPos2.x) / (worldPos1.x - worldPos2.x);
            float b = minimapPos1.x - a * worldPos1.x;
            float c = (minimapPos1.y - minimapPos2.y) / (worldPos1.y - worldPos2.y);
            float d = minimapPos1.y - c * worldPos1.y;

            // Apply inverse transformation to target minimap position
            float targetWorldX = (targetMinimapPos.x - b) / a;
            float targetWorldY = (targetMinimapPos.y - d) / c;

            return new Vector2(targetWorldX, targetWorldY);
        }


        public void OnScroll(PointerEventData eventData)
        {
            if (circleMap)
            {
                return;
            }

            RawImage rawImage = miniMap;
            var uvRect = rawImage.uvRect;



            float mouseWheelRotation = -eventData.scrollDelta.y / 50f;


            if (zoomfinal <= 0.2 && mouseWheelRotation < 0)
            {
                return;
            }

            if (zoomfinal >= 1 && mouseWheelRotation > 0)
            {
                return;
            }


            if (mouseWheelRotation != 0)
            {
                zoomfinal += mouseWheelRotation;
                //uvRect.height = uvRect.width;
                //rawImage.uvRect = uvRect;

                return;

                Vector2 localCursor;
                // แปลงตำแหน่งคลิกของเมาส์เป็น local position ภายใน rawImage
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(miniMap.rectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
                    return;

                float normalizedX = -(localCursor.x / miniMap.rectTransform.rect.width) + focusPoint.x;
                float normalizedY = -(localCursor.y / miniMap.rectTransform.rect.height) + focusPoint.y;

                Vector2 beforePositon = new Vector2(normalizedX, normalizedY);
                Debug.Log("*************" + uvRect.width / (uvRect.width + mouseWheelRotation));
                Vector2 afterPositon = CalculateExpandedPosition(beforePositon, Vector2.one * 0.5f, uvRect.width / (uvRect.width + mouseWheelRotation));

                Vector2 deltaPosition = (beforePositon - afterPositon) / (uvRect.width / (uvRect.width + mouseWheelRotation));//

                uvRect.width += mouseWheelRotation;
                uvRect.height = uvRect.width;
                rawImage.uvRect = uvRect;

                focusPoint.x += deltaPosition.x;
                focusPoint.y += deltaPosition.y;
                MiniMapPoint = focusPoint;


            }
        }


        Vector2 CalculateExpandedPosition(Vector2 originalPosition, Vector2 centerOfScaling, float scaleRate)
        {
            Vector2 direction = originalPosition - centerOfScaling; // หาทิศทางและระยะห่างจากจุดศูนย์กลาง
            Vector2 scaledDirection = direction * scaleRate; // คูณด้วยอัตราการขยาย
            Vector2 newPosition = centerOfScaling + scaledDirection; // คำนวณตำแหน่งใหม่

            return newPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (circleMap)
            {
                return;
            }

            isDragging = true;
            float rectWidth = miniMap.rectTransform.rect.width;
            float rectHeight = miniMap.rectTransform.rect.height;


            float normalizedX = -(eventData.delta.x / (rectWidth) * miniMap.uvRect.width) + focusPoint.x; // แปลงค่า X เป็น normalized
            float normalizedY = -(eventData.delta.y / (rectHeight) * miniMap.uvRect.height) + focusPoint.y; // แปลงค่า Y เป็น normalized
            focusPoint = new Vector2(normalizedX, normalizedY);
            //MiniMapPoint = new Vector2(normalizedX, normalizedY);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = false;

        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (circleMap)
            {
                return;
            }

            if (isDragging)
            {
                return;
            }

            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (Data.GetIconByName("pinpoint")[0].Group == IconGroup.NONE)
            {
                Data.GetIconByName("pinpoint")[0].Group = IconGroup.PINPOINT;
            }

            Vector2 localCursor;
            // แปลงตำแหน่งคลิกของเมาส์เป็น local position ภายใน rawImage
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(miniMap.rectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
                return;


            float rectWidth = miniMap.rectTransform.rect.width;
            float rectHeight = miniMap.rectTransform.rect.height;

            float normalizedX = (localCursor.x / (rectWidth) * miniMap.uvRect.width) + focusPoint.x;
            float normalizedY = (localCursor.y / (rectHeight) * miniMap.uvRect.height) + focusPoint.y;


            Data.GetIconByName("pinpoint")[0].Position = ConvertMinimapToWorld(Data.WorldPoint[0], Data.WorldPoint[1], Data.MapPoint[0], Data.MapPoint[1], new Vector2(normalizedX, normalizedY));
            signalBus.Fire(new RefreshIconSignal());

        }
    }
}
