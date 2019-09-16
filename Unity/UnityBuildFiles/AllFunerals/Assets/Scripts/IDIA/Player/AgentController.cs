// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;
using DrupalUnity;

/// <summary>
/// This class handles the movement of the nav mesh agent on the nav mesh.
/// </summary>
public class AgentController : MonoBehaviour {

    #region Fields
    /// <summary>
    /// The nav mesh agent.
    /// </summary>
    public UnityEngine.AI.NavMeshAgent navMeshAgent;
    /// <summary>
    /// The destination of this nav mesh agent.
    /// </summary>
    Vector3 destination;
    #endregion

    #region Unity Messages
    /// <summary>
    /// A message called when the script is enabled.
    /// </summary>
    void OnEnable() {
        DrupalUnityIO.OnPlacardSelected += OnPlacardSelected;
    }
    /// <summary>
    /// A message called when the script updates.
    /// </summary>
    void Update () {
        if(navMeshAgent.isOnNavMesh) {
            if(!navMeshAgent.pathPending) {
                navMeshAgent.SetDestination(destination);
                if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
                    if(!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f) {
                        navMeshAgent.Stop();
                    }
                }
            }
        }
	}
    /// <summary>
    /// A message called when the script is disabled.
    /// </summary>
    void OnDisable() {
        DrupalUnityIO.OnPlacardSelected -= OnPlacardSelected;
    }
    #endregion

    #region Callbacks
    /// <summary>
    /// A callback called when a placard is selected in the Drupal Unity Interface.
    /// </summary>
    /// <param name="placard"></param>
    void OnPlacardSelected(Placard placard) {
        destination = GeographicManager.Instance.GetPosition(placard.location.latitude, placard.location.longitude, placard.location.elevation);
        navMeshAgent.Resume();
    }
    #endregion

}
