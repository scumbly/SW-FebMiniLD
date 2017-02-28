using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/NPR/Control/JointFadeController")]
[RequireComponent(typeof(NprEffects))]
/// <summary>
/// Utility script that can be used to control all the fade-type effects 
/// 	(e.g. edge-thickness, simplification, desaturation) together, i.e. this script controls 
/// 	the FadeControl attribute of the Edges, Simplify and Desaturate effects
/// 	when their corresponding "fade" attributes are enabled.
/// This way the changes of the level of abstraction can be controlled by a single script.
/// </summary>
public class NprJointFadeController : MonoBehaviour
{

    public NprFadeControl fadeControl = null;

    protected NprEffects effects;
    protected NprFadeControl fadeControlEdges;
    protected NprFadeControl fadeControlSimplification;
    protected NprFadeControl fadeControlDesaturation;

    void Start()
    {
        if (fadeControl == null)
        {
            fadeControl = new NprFadeControl();
        }
    }

    void Update()
    {
        effects = gameObject.GetComponent<NprEffects>();
        fadeControlEdges = effects.fadeControlEdges;
        fadeControlSimplification = effects.fadeControlSimplification;
        fadeControlDesaturation = effects.fadeControlDesaturation;

        fadeControlEdges.copy(fadeControl);
        fadeControlSimplification.copy(fadeControl);
        fadeControlDesaturation.copy(fadeControl);
    }
}
