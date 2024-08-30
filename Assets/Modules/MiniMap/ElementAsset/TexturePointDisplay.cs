
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TexturePointDisplay : MonoBehaviour, IPointerMoveHandler, IPointerDownHandler
{
    public RawImage rawImage;
    public RawImage rawImageOut;
    public PointerEventData backupPoint;
    public void CenterTextureAt(float tu, float tv)
    {
        RawImage rawImage = rawImageOut;
        if (rawImage == null)
            return;

        // กำหนดขนาดของ uvRect
        float width = rawImage.uvRect.width;
        float height = rawImage.uvRect.height;

        // คำนวณเพื่อหา offset ใหม่ โดยให้ texture อยู่ตรงกลาง
        float offsetX = tu - (width / 2f);
        float offsetY = tv - (height / 2f);

        // สร้าง uvRect ใหม่ที่จะเลื่อน texture ไปยังตำแหน่งที่ต้องการและมีขนาดตามที่กำหนด
        rawImage.uvRect = new Rect(offsetX, offsetY, width, height);
    }

    void Update()
    {
        float mouseWheelRotation = Input.GetAxis("Mouse ScrollWheel");

        if (mouseWheelRotation != 0)
        {
            RawImage rawImage = rawImageOut;
            var uvRect = rawImage.uvRect;
            uvRect.width += mouseWheelRotation;
            uvRect.height = uvRect.width;
            rawImage.uvRect = uvRect;
            Drag(backupPoint);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0))
        {
            Drag(eventData);
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0))
        {
            Drag(eventData);
        }
    }

    public void Drag(PointerEventData eventData)
    {
        Vector2 localCursor;
        backupPoint = new PointerEventData(EventSystem.current);
        backupPoint.position = new Vector2(eventData.position.x, eventData.position.y);
        // แปลงตำแหน่งคลิกของเมาส์เป็น local position ภายใน rawImage
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.rectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
            return;
        // คำนวณ normalized position ภายใน rawImage
        float rectWidth = rawImage.rectTransform.rect.width;
        float rectHeight = rawImage.rectTransform.rect.height;
        float normalizedX = (localCursor.x / rectWidth) + 0.5f; // แปลงค่า X เป็น normalized
        float normalizedY = (localCursor.y / rectHeight) + 0.5f; // แปลงค่า Y เป็น normalized

        Debug.Log($"Texture Coordinates: TU={normalizedX}, TV={normalizedY}");
        CenterTextureAt(normalizedX, normalizedY);
    }

}
