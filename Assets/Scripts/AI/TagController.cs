using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class TagController : MonoBehaviour
    {
        public static TagController instance;
        public List<TagPlayer> players;
        public TagPlayer LastFrozen;
        public GameObject winScreen;
        private bool GameOn = false;

        void OnEnable() {
            instance = this;
            transform.parent.GetComponentsInChildren<TagPlayer>(players);   // Get all players
            int whosIt = Random.Range(0, players.Count);                    // Set a random player as "it"
            players[whosIt].isIt = true;
            foreach (TagPlayer player in players) {
                player.It = players[whosIt];
            }

            players[whosIt].transform.Find("Tophat").gameObject.SetActive(true);
            players[whosIt].transform.GetComponent<AIAgent>().hasSpeedBoost = true;
            GameOn = true;
        }

        // Get the closest player to another player. Optionally include frozen players.
        public TagPlayer GetClosestPlayer(TagPlayer self, bool includeFrozen = false) {
            bool firstPass = true;
            float closestDistance = float.PositiveInfinity;
            TagPlayer closest = null;

            foreach (TagPlayer player in players) {
                // Do not return oneself
                if (player == self) {
                    continue;
                }
                // Skip frozen players unless told otherwise
                if (includeFrozen == false && player.IsFrozen() == true) {
                    continue;
                }

                if (firstPass) {
                    // On the first pass, set the first found player as closeset
                    closest = player;
                    closestDistance = (closest.transform.position - self.transform.position).magnitude;
                    firstPass = false;
                } else {
                    // On all subsequent passes, check to see if the new player is closer than the current closest
                    float distance = (player.transform.position - self.transform.position).magnitude;
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closest = player;
                    }
                }
            }
            return closest;
        }

        // Get the nearest player to untag. Generally used by idle Tag players. Ignores targets.
        public TagPlayer GetClosestFrozenPlayer(TagPlayer self) {
            bool firstPass = true;
            float closestDistance = float.PositiveInfinity;
            TagPlayer closest = null;

            foreach (TagPlayer player in players) {
                // Do not return oneself
                if (player == self) {
                    continue;
                }
                // Skip frozen players unless told otherwise
                if (player.IsFrozen() == false) {
                    continue;
                }
                // Someone else is already on the way to untag this player
                if (player.isTarget == true) {
                    continue;
                }

                if (firstPass) {
                    // On the first pass, set the first found player as closeset
                    closest = player;
                    closestDistance = (closest.transform.position - self.transform.position).magnitude;
                } else {
                    // On all subsequent passes, check to see if the new player is closer than the current closest
                    float distance = (player.transform.position - self.transform.position).magnitude;
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closest = player;
                    }
                }
            }
            return closest;
        }

        public void ResetGame() 
        {
            if (GameOn == false) {
                return;
            }
            GameOn = false;
            StartCoroutine(ResetCoroutine());
        }

        private IEnumerator ResetCoroutine() {
            winScreen.SetActive(true);
            yield return new WaitForSeconds(GameConstants.GAME_RESET_TIME);
            winScreen.SetActive(false);

            TagPlayer lastIt = LastFrozen.It;
            
            // Remove "it" status from last It
            lastIt.isIt = false;
            lastIt.transform.Find("Tophat").gameObject.SetActive(false);
            lastIt.transform.GetComponent<AIAgent>().hasSpeedBoost = false;

            // Set the last frozen player as the new It
            LastFrozen.isIt = true;
            LastFrozen.transform.Find("Tophat").gameObject.SetActive(true);
            LastFrozen.transform.GetComponent<AIAgent>().hasSpeedBoost = true;

            // Tell all players who is it
            foreach (TagPlayer player in players) {
                player.SetFreeze(false);
                player.It = LastFrozen;
            }
            GameOn = true;
        }
    }
}
