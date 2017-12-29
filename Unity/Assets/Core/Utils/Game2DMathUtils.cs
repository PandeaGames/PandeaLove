using UnityEngine;

public class Game2DMathUtils
{
    private static void CalculateDeltaFromCross(Vector3 cross, out float Rad)
    {
        Rad = Mathf.PI - Mathf.Abs(cross.z);
    }

    private static void CalculateDeltaFromCross(Vector3 cross, out float Rad, out float Ratio)
    {
        Rad = Mathf.PI - Mathf.Abs(cross.z);
        Ratio = Rad / Mathf.PI;
    }

    private static void CalculateDeltaFromCross(Vector3 cross, out float Rad, out float Deg, out float Ratio)
    {
        Rad = Mathf.PI - Mathf.Abs(cross.z);
        Ratio = Rad / Mathf.PI;
        Deg = Rad * Mathf.Rad2Deg;
    }

    public static void ApplyTorque(Transform transform, Vector3 target, Rigidbody2D body, float torque, out Vector3 cross)
    {
        Vector3 targetDelta = target - transform.position;

        //get the angle between transform.forward and target delta
        float angleDiff = Vector3.Angle(transform.right, targetDelta);

        // get its cross product, which is the axis of rotation to
        // get from one vector to the other
        cross = Vector3.Cross(transform.right, targetDelta);

        // apply torque along that axis according to the magnitude of the angle.
        body.AddTorque(cross.z * angleDiff * torque);
    }

    public static void ApplyTorqueAndForce(Transform fromTransform, Vector3 toTarget, Rigidbody2D body, float torque, float force)
    {
        Vector3 cross;

        ApplyTorque(fromTransform, toTarget, body, torque, out cross);

        float Rad;
        float Ratio;

        CalculateDeltaFromCross(cross, out Rad, out Ratio);

        force *= Ratio;

        body.AddForce(fromTransform.right * force, ForceMode2D.Force);
    }

    public static void ApplyTorqueAndForce(Transform fromTransform, Vector3 toTarget, Rigidbody2D body, float torque, float force, AnimationCurve rotationCurve)
    {
        Vector3 cross;

        ApplyTorque(fromTransform, toTarget, body, torque, out cross);

        float Rad;
        float Ratio;

        CalculateDeltaFromCross(cross, out Rad, out Ratio);

        force *= rotationCurve.Evaluate(Ratio);

        body.AddForce(fromTransform.right * force, ForceMode2D.Force);
    }

    public static void ApplyTorqueAndForce(Transform fromTransform, Vector3 toTarget, Rigidbody2D body, float torque, float force, AnimationCurve rotationCurve, AnimationCurve distanceCurve, float distanceClamp)
    {
        Vector3 cross;
        float dist = Vector3.Distance(toTarget, fromTransform.position);

        ApplyTorque(fromTransform, toTarget, body, torque, out cross);

        float Rad;
        float Ratio;

        CalculateDeltaFromCross(cross, out Rad, out Ratio);

        force *= rotationCurve.Evaluate(Ratio);
        force *= distanceCurve.Evaluate(Mathf.Clamp(dist, 0, distanceClamp) / distanceClamp);

        body.AddForce(fromTransform.right * force, ForceMode2D.Force);
    }

    public static void ApplyTorqueAndForce(Transform fromTransform, Vector3 toTarget, Rigidbody2D body, float torque, float force, AnimationCurve distanceCurve, float distanceClamp)
    {
        Vector3 cross;
        float dist = Vector3.Distance(toTarget, fromTransform.position);

        ApplyTorque(fromTransform, toTarget, body, torque, out cross);

        float Rad;
        float Ratio;

        CalculateDeltaFromCross(cross, out Rad, out Ratio);

        force *= Ratio;
        force *= distanceCurve.Evaluate(Mathf.Clamp(dist, 0, distanceClamp) / distanceClamp);

        body.AddForce(fromTransform.right * force, ForceMode2D.Force);
    }

    public static void LookAt(Transform fromTransform, Vector3 toTarget)
    {
        Vector3 dir = toTarget - fromTransform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        fromTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}