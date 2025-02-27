﻿using UnityEngine;
using UnityEditor;

namespace JacDev.Utils
{
    [CustomEditor(typeof(BezierCurve))]
    public class BezierCurveInspector : Editor
    {
        BezierCurve curve;
        Transform handleTransform;
        Quaternion handleRotation;

        // 曲線平滑度
        const int lineSteps = 10;
        const float directionScale = .5f;

        private void OnSceneGUI()
        {
            curve = target as BezierCurve;
            handleTransform = curve.transform;
            handleRotation = Tools.pivotRotation == PivotRotation.Local ?
                handleTransform.rotation : Quaternion.identity;

            // 顯示三個點
            Vector3 p0 = ShowPoint(0);
            Vector3 p1 = ShowPoint(1);
            Vector3 p2 = ShowPoint(2);
            Vector3 p3 = ShowPoint(3);

            // 渲染直線
            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            ShowDirections();

            Handles.DrawBezier(p0, p3, p1, p2, Color.cyan, null, 2f);
        }

        // 顯示點
        Vector3 ShowPoint(int index)
        {
            Vector3 point = handleTransform.TransformPoint(curve.points[index]);
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(curve, "Move Point");
                EditorUtility.SetDirty(curve);
                curve.points[index] = handleTransform.InverseTransformPoint(point);
            }

            return point;
        }

        void ShowDirections()
        {
            Handles.color = Color.yellow;
            Vector3 point = curve.GetPoint(0f);

            Handles.DrawLine(point, point + curve.GetDirection(0f) * directionScale);
            for (int i = 1; i <= lineSteps; ++i)
            {
                point = curve.GetPoint(i / (float)lineSteps);
                Handles.DrawLine(
                    point, 
                    point + curve.GetDirection(i / (float)lineSteps) * directionScale);
            }
        }
    }
}

