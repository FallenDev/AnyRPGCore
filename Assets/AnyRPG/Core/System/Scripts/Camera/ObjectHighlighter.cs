using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;


namespace AnyRPG {

	[RequireComponent(typeof(Camera))]
	public class ObjectHighlighter : MonoBehaviour {
		
		private Color outlineColor = new Color(1, 0, 0, 0.5f);
		public Camera cam;

		private Renderer[] meshRenderers = null;
		private List<uint> originalRenderingLayers = new List<uint>();

		private void Awake() {
			//cam.depthTextureMode = DepthTextureMode.Depth;
		}

		void OnDisable() {
			Cleanup();
		}

		void Cleanup() {
		}

		public void AddOutlinedObject(Interactable interactable, Color outlineColor, Renderer[] meshRenderers) {
			//Debug.Log($"ObjectHighlighter.AddOutlinedObject({interactable.gameObject.name}, {outlineColor})");

			this.outlineColor = outlineColor;
			this.meshRenderers = meshRenderers;

            OutlineVolumeComponent outlineVolume = VolumeManager.instance.stack.GetComponent<OutlineVolumeComponent>();
			outlineVolume.color1.value = outlineColor;

            originalRenderingLayers.Clear();
            // loop through mesh renderers and add Outline_1 to their rendering layers
            foreach (Renderer renderer in meshRenderers) {
                // store original rendering layer
                originalRenderingLayers.Add(renderer.renderingLayerMask);
                // get rendering layer by name
                int outlineLayer = RenderingLayerMask.NameToRenderingLayer("Outline_1");
                // add outline layer to existing rendering layers
				renderer.renderingLayerMask = renderer.renderingLayerMask | (1u << outlineLayer);
            }
        }

        public void RemoveOutlinedObject(Interactable interactable) {
            //Debug.Log($"ObjectHighlighter.RemoveOutlinedObject({interactable.gameObject.name})");

			if (meshRenderers == null) {
				return;
			}
            // restore original rendering layers
            for (int i = 0; i < meshRenderers.Length; i++) {
                if (i < originalRenderingLayers.Count) {
                    meshRenderers[i].renderingLayerMask = originalRenderingLayers[i];
                }
            }

            originalRenderingLayers.Clear();

			meshRenderers = null;
		}
		
	}

}