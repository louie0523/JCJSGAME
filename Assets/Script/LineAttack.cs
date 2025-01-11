using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineAttack : MonoBehaviour
{
    public GameObject attackBox;
    public LineRenderer linePrefab;
    public GameObject crossTextPrefab;
    public GameObject TriangleText;
    public GameObject SquareText;
    public GameObject PantagonText;
    public float lineDuration = 1f;
    public float textDuration = 1f;
    public float bossDamageMultiplier = 0.5f;
    public float crossDamageMultiplier = 0.5f;
    public float triangleDamageMultiplier = 1.0f;
    public float squareDamageMultiplier = 1.5f;
    public float pentagonDamageMultiplier = 2.0f;

    private Vector3 startMousePosition;
    private Vector3 endMousePosition;
    private bool isDrawing = false;
    private LineRenderer currentLineRenderer;

    private List<LineRenderer> lines = new List<LineRenderer>();
    private List<LineRenderer> crossedLines = new List<LineRenderer>();
    private List<Vector3> intersectionPoints = new List<Vector3>();

    void Start()
    {
        EdgeCollider2D edgeCollider = linePrefab.gameObject.GetComponent<EdgeCollider2D>();
        if (edgeCollider != null)
        {
            edgeCollider.isTrigger = true;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            if (IsInsideAttackBox(mouseWorldPosition))
            {
                startMousePosition = mouseWorldPosition;
                isDrawing = true;

                currentLineRenderer = Instantiate(linePrefab, transform);
                currentLineRenderer.positionCount = 2;
                currentLineRenderer.SetPosition(0, startMousePosition);
                currentLineRenderer.SetPosition(1, startMousePosition);
            }
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            if (IsInsideAttackBox(mouseWorldPosition))
            {
                currentLineRenderer.SetPosition(1, mouseWorldPosition);
            }
            else
            {
                Vector3 clampedPosition = ClampPositionToAttackBox(mouseWorldPosition);
                currentLineRenderer.SetPosition(1, clampedPosition);
            }
        }

        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            isDrawing = false;

            endMousePosition = GetMouseWorldPosition();
            endMousePosition = ClampPositionToAttackBox(endMousePosition);
            currentLineRenderer.SetPosition(1, endMousePosition);

            lines.Add(currentLineRenderer);

            StartCoroutine(ClearLineAfterDelay(currentLineRenderer, lineDuration));

            ApplyDamageToBoss();

            CheckForLineCrossing();
            DetectShape();
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10f;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    private bool IsInsideAttackBox(Vector3 position)
    {
        return attackBox.GetComponent<Collider2D>().bounds.Contains(position);
    }

    private Vector3 ClampPositionToAttackBox(Vector3 position)
    {
        Bounds boxBounds = attackBox.GetComponent<Collider2D>().bounds;

        float clampedX = Mathf.Clamp(position.x, boxBounds.min.x, boxBounds.max.x);
        float clampedY = Mathf.Clamp(position.y, boxBounds.min.y, boxBounds.max.y);

        return new Vector3(clampedX, clampedY, position.z);
    }

    private IEnumerator ClearLineAfterDelay(LineRenderer line, float delay)
    {
        yield return new WaitForSeconds(delay);
        lines.Remove(line);
        Destroy(line.gameObject);
    }

    private void ApplyDamageToBoss()
    {
        float lineLength = Vector3.Distance(currentLineRenderer.GetPosition(0), currentLineRenderer.GetPosition(1));
        float damage = lineLength * bossDamageMultiplier;

        GameManager.Instance.UpdateBossHP(GameManager.Instance.BossHP - damage);

        Debug.Log($"보스에게 입힌 피해: {damage}, 남은 보스 체력: {GameManager.Instance.BossHP}");
    }

    private void CheckForLineCrossing()
    {
        foreach (var line in lines)
        {
            foreach (var otherLine in lines)
            {
                if (line != otherLine && !crossedLines.Contains(line) && !crossedLines.Contains(otherLine))
                {
                    Vector3 intersectionPoint = GetIntersection(line.GetPosition(0), line.GetPosition(1), otherLine.GetPosition(0), otherLine.GetPosition(1));
                    if (intersectionPoint != Vector3.zero)
                    {
                        ShowCrossText(intersectionPoint);
                        ApplyCrossDamage(line, otherLine, intersectionPoint);

                        crossedLines.Add(line);
                        crossedLines.Add(otherLine);
                    }
                }
            }
        }
    }

    private void DetectShape()
    {
        intersectionPoints.Clear();

        // Find intersections
        for (int i = 0; i < lines.Count; i++)
        {
            for (int j = i + 1; j < lines.Count; j++)
            {
                Vector3 intersection = GetIntersection(
                    lines[i].GetPosition(0), lines[i].GetPosition(1),
                    lines[j].GetPosition(0), lines[j].GetPosition(1));

                if (intersection != Vector3.zero && !ContainsPoint(intersectionPoints, intersection))
                {
                    intersectionPoints.Add(intersection);
                }
            }
        }

        // Check for shapes
        if (IsClosedShape(3)) ApplyShapeDamage(triangleDamageMultiplier, "Triangle");
        if (IsClosedShape(4)) ApplyShapeDamage(squareDamageMultiplier, "Square");
        if (IsClosedShape(5)) ApplyShapeDamage(pentagonDamageMultiplier, "Pentagon");
    }

    private bool IsClosedShape(int vertexCount)
    {
        if (intersectionPoints.Count < vertexCount) return false;

        int connections = 0;
        foreach (var point in intersectionPoints)
        {
            foreach (var otherPoint in intersectionPoints)
            {
                if (point != otherPoint && AreConnected(point, otherPoint))
                {
                    connections++;
                }
            }
        }

        return connections / 2 == vertexCount;
    }

    private bool AreConnected(Vector3 point1, Vector3 point2)
    {
        foreach (var line in lines)
        {
            if (IsPointOnLine(point1, line) && IsPointOnLine(point2, line))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsPointOnLine(Vector3 point, LineRenderer line)
    {
        Vector3 lineStart = line.GetPosition(0);
        Vector3 lineEnd = line.GetPosition(1);

        float lineLength = Vector3.Distance(lineStart, lineEnd);
        float distanceToStart = Vector3.Distance(point, lineStart);
        float distanceToEnd = Vector3.Distance(point, lineEnd);

        return Mathf.Abs(lineLength - (distanceToStart + distanceToEnd)) < 0.1f;
    }

    private bool ContainsPoint(List<Vector3> points, Vector3 point)
    {
        foreach (var existingPoint in points)
        {
            if (Vector3.Distance(existingPoint, point) < 0.1f) return true;
        }
        return false;
    }

    private void ApplyShapeDamage(float multiplier, string shapeName)
    {
        float totalDamage = intersectionPoints.Count * multiplier;
        GameManager.Instance.UpdateBossHP(GameManager.Instance.BossHP - totalDamage);

        Debug.Log($"{shapeName} 피해: {totalDamage}, 남은 보스 체력: {GameManager.Instance.BossHP}");

        // 도형의 중앙 계산
        Vector3 center = CalculateShapeCenter(intersectionPoints);

        // 도형 중앙에 텍스트 표시
        GameObject shapeTextPrefab = GetShapeTextPrefab(shapeName);
        if (shapeTextPrefab != null)
        {
            ShowShapeText(center, shapeTextPrefab);
        }
    }
    // 도형의 중앙 계산
    private Vector3 CalculateShapeCenter(List<Vector3> points)
    {
        Vector3 center = Vector3.zero;
        foreach (var point in points)
        {
            center += point;
        }
        center /= points.Count; // 평균값 계산
        return center;
    }

    // 도형에 따른 텍스트 프리팹 선택
    private GameObject GetShapeTextPrefab(string shapeName)
    {
        switch (shapeName)
        {
            case "Triangle": return TriangleText;
            case "Square": return SquareText;
            case "Pentagon": return PantagonText;
            default: return null;
        }
    }

    // 텍스트 표시
    private void ShowShapeText(Vector3 position, GameObject textPrefab)
    {
        GameObject textInstance = Instantiate(textPrefab, position, Quaternion.identity);
        StartCoroutine(ClearTextAfterDelay(textInstance, textDuration));
    }
    private Vector3 GetIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        float x1 = p1.x, y1 = p1.y;
        float x2 = p2.x, y2 = p2.y;
        float x3 = p3.x, y3 = p3.y;
        float x4 = p4.x, y4 = p4.y;

        float denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (denominator == 0) return Vector3.zero;

        float t1 = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denominator;
        float t2 = ((x1 - x3) * (y1 - y2) - (y1 - y3) * (x1 - x2)) / denominator;

        if (t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1)
        {
            float intersectionX = x1 + t1 * (x2 - x1);
            float intersectionY = y1 + t1 * (y2 - y1);
            return new Vector3(intersectionX, intersectionY, 0);
        }
        return Vector3.zero;
    }

    private void ShowCrossText(Vector3 position)
    {
        GameObject textInstance = Instantiate(crossTextPrefab, position, Quaternion.identity);
        StartCoroutine(ClearTextAfterDelay(textInstance, textDuration));
    }

    private IEnumerator ClearTextAfterDelay(GameObject textObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(textObject);
    }

    private void ApplyCrossDamage(LineRenderer line1, LineRenderer line2, Vector3 intersectionPoint)
    {
        float distance1 = Vector3.Distance(line1.GetPosition(0), intersectionPoint) + Vector3.Distance(line1.GetPosition(1), intersectionPoint);
        float distance2 = Vector3.Distance(line2.GetPosition(0), intersectionPoint) + Vector3.Distance(line2.GetPosition(1), intersectionPoint);

        float totalLength = distance1 + distance2;
        float crossDamage = totalLength * crossDamageMultiplier;

        GameManager.Instance.UpdateBossHP(GameManager.Instance.BossHP - crossDamage);
        Debug.Log($"교차 피해: {crossDamage}");
    }
}
