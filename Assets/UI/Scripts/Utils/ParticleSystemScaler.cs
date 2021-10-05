using UnityEngine;

[ExecuteAlways]
public class ParticleSystemScaler : MonoBehaviour
{
    private void LateUpdate()
    {
        var system = GetComponent<ParticleSystem>();
        if (system != null && Camera.main != null)
        {
            // Position
            var source = Camera.main.transform.position;
            source.z = system.transform.position.z;
            system.transform.position = source;

            // Size
            var systemShape = system.shape;
            systemShape.shapeType = ParticleSystemShapeType.Rectangle;
            systemShape.scale = new Vector3(
                Camera.main.orthographicSize * 2 * Camera.main.aspect,
                Camera.main.orthographicSize * 2,
                1
            );
        }
    }
}
