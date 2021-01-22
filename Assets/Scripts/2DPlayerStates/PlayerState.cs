﻿///<summary>
/// Created by Paul O'Callaghan
/// PlayerState Will handle all types of situations a player can be in such as touching the floor walls.
/// Scale to use assumes transform.localScale.x is always 1(facing right) or -1(facing left) for sprite facing direction
/// and placement of raycasts
/// 
/// 
/// ToDo:
/// Add in other terrain types such as Ice or touching water
/// 
/// 
/// </summary>


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TwoDTools
{
    public class PlayerState : MonoBehaviour
    {
        private bool touchingWall;
        private bool touchingFloor;
        private bool touchingCeiling;
        private bool touchingWallBehind;

        private bool touchingSlopeFront;
        private bool touchingSlopeBack;

        private bool isJumping;



        // Player Game State
        private bool isDead = false;
        private bool isFinishedLevel = false;
        private bool isSprinting = false;

        public SpriteRenderer spriteRenderer;
        private TwoDTools.PlayerController2D playerController;

        // Mini optimisation
        Transform myTransform;

        // used by TouchingTerrain()
        Vector2 offset;

        public float slopeAngleFront;
        public float slopeAngleBack;
        public float touchWallAngle = 0;
        public const float MAX_SLOPE_LIMIT = 80;
        public Vector2 hitPointBack;
        public Vector2 hitPointFront;

        enum CheckType
        {
            Horizontal,
            Vertical
        }

        enum SlopeCheck
        {
            None,
            Front,
            Back
        }

        public void Jumping()
        {
            isJumping = true;
        }

        public void NotJumping()
        {
            isJumping = false;
        }

        public bool IsJumping()
        {
            return isJumping;
        }

        public void Awake()
        {
            ResetAllStates();
            myTransform = transform;
            playerController = GetComponent<TwoDTools.PlayerController2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void UpdatePlayerState()
        {

            // Horizontal Checks
            WallCheck();
            
            touchingWallBehind = TouchingTerrain(-myTransform.right * Mathf.Sign(myTransform.localScale.x), CheckType.Horizontal, playerController.playerControllerData.horizontalRaycasts, playerController.playerControllerData.raycastLengthHorizontal, SlopeCheck.None);
            
            // Vertical Checks
            touchingCeiling = TouchingTerrain(myTransform.up, CheckType.Vertical, playerController.playerControllerData.verticalRaycasts, playerController.playerControllerData.raycastLengthVertical, SlopeCheck.None);
            touchingFloor = TouchingTerrain(-myTransform.up, CheckType.Vertical, playerController.playerControllerData.verticalRaycasts, playerController.playerControllerData.raycastLengthVertical, SlopeCheck.None);

            // Slope Front
            TouchingTerrain(-myTransform.up, CheckType.Vertical, 1, playerController.playerControllerData.raycastLengthHorizontal * 3f, SlopeCheck.Front);
            // Slope Back
            TouchingTerrain(-myTransform.up, CheckType.Vertical, 1, playerController.playerControllerData.raycastLengthHorizontal * 1.6f, SlopeCheck.Back);

        }

        private void WallCheck()
        {
            touchingWall = TouchingTerrain(myTransform.right * Mathf.Sign(myTransform.localScale.x), CheckType.Horizontal, playerController.playerControllerData.horizontalRaycasts, playerController.playerControllerData.raycastLengthHorizontal, SlopeCheck.None);
            if (touchingWall)
            {
                if (touchWallAngle <= MAX_SLOPE_LIMIT && touchWallAngle > 0)
                {
                    touchingWall = false;
                }
            }
        }


        public void ResetTouchWalls()
        {
            touchingWall = false;
            touchingWallBehind = false;
        }

        public bool IsTouchingWall()
        {
            return touchingWall;
        }
        public bool IsTouchingWallBehind()
        {
            return touchingWallBehind;
        }


        public bool IsTouchingCeiling()
        {
            return touchingCeiling;
        }

        public bool IsTouchingFloor()
        {
            return touchingFloor;
        }

        public void SetIsTouchingFloor(bool touchingFloor)
        {
            this.touchingFloor = touchingFloor;
        }

        public void ResetAllStates()
        {
            touchingWall = false;
            touchingFloor = false;
            touchingCeiling = false;
            touchingWallBehind = false;
            isDead = false;
            isSprinting = false;
            touchingSlopeFront = false;
            touchingSlopeBack = false;
        }
        public bool IsTouchingSlope()
        {
            if(touchingSlopeFront == true)
            {
                return true;
            }
            if(touchingSlopeBack == true)
            {
                return true;
            }
            return false;
        }
        public bool IsTouchingSlopeFront()
        {
            return touchingSlopeFront;
        }
        public bool IsTouchingSlopeBack()
        {
            return touchingSlopeBack;
        }

        public bool FacingDownSlope()
        {
            if (!touchingSlopeFront && !touchingSlopeBack)
            {
                return false;
            }
            if (touchingSlopeFront && !touchingSlopeBack)
            {
                if (hitPointFront.y > hitPointBack.y)
                {
                    return false;
                }
                return true;
            }
            if (hitPointFront.y > hitPointBack.y)
            {
                return false;
            }
            return true;
        }

        public void SetIsTouchingSlopeFront(bool isTouchingSlope)
        {
            touchingSlopeFront = isTouchingSlope;
        }
        public void SetIsTouchingSlopeBack(bool isTouchingSlope)
        {
            touchingSlopeBack = isTouchingSlope;
        }

        public void ResetTouchingSlope()
        {
            touchingSlopeFront = false;
        }

        public bool GetIsDead()
        {
            return isDead;
        }

        public bool GetIsFinishedLevel()
        {
            return isFinishedLevel;
        }

        public void SetDeadAs(bool state)
        {
            isDead = state;
        }
        public void Kill()
        {
            isDead = true;
        }

        public bool IsMovingHorizontal()
        {
            return ((playerController.input.RightButton() && playerController.currentVelocity.x > 0) || (playerController.input.LeftButton() && playerController.currentVelocity.x < 0));
        }

        public bool IsMovingVerticalUp()
        {
            if (touchingFloor)
            {
                return false;
            }
            return (playerController.currentVelocity.y > 0);
        }
        public bool IsMovingVerticalDown()
        {
            if (touchingFloor)
            {
                return false;
            }
            return (playerController.currentVelocity.y < 0);
        }


        bool CheckSlopeAngle(SlopeCheck slopeCheck)
        {
            switch(slopeCheck)
            {
                case SlopeCheck.Front:
                    if (slopeAngleFront <= MAX_SLOPE_LIMIT && slopeAngleFront > 0)
                    {
                        touchingSlopeFront = true;
                        return true;
                    }
                    touchingSlopeFront = false;
                    break;

                case SlopeCheck.Back:
                    if (slopeAngleBack <= MAX_SLOPE_LIMIT && slopeAngleBack > 0)
                    {
                        touchingSlopeBack = true;
                        return true;
                    }
                    touchingSlopeBack = false;
                    break;

                default:
                case SlopeCheck.None:
                    break;
            }
            return false;
        }

        private bool TouchingTerrain(Vector2 direction, CheckType checkType, int numberOfRaycasts, float length, SlopeCheck slopeCheck)
        {

#if UNITY_EDITOR
            // Only used with debug setup for displaying hit raycasts
            bool hit = false;
#endif
            switch (checkType)
            {
                case CheckType.Vertical:
                    offset = new Vector2(spriteRenderer.size.x, 0);
                    break;
                case CheckType.Horizontal:
                    offset = new Vector2(0, spriteRenderer.size.y);
                    break;
            }

            Vector2 minimumBounds = -offset / 2.21f;
            Vector2 maximumBounds = offset / 2.21f;

            switch (checkType)
            {
                case CheckType.Vertical:
                    minimumBounds.x -= playerController.playerControllerData.raycastSpreadAmountVertical;
                    maximumBounds.x += playerController.playerControllerData.raycastSpreadAmountVertical;
                    break;
                case CheckType.Horizontal:
                    minimumBounds.y -= playerController.playerControllerData.raycastSpreadAmountHorizontal;
                    maximumBounds.y += playerController.playerControllerData.raycastSpreadAmountHorizontal;
                    break;
            }

            for (int i = 0; i < numberOfRaycasts; i++)
            {

                Vector2 pos = new Vector2();
                if (numberOfRaycasts > 1)
                {
                    pos = Vector2.Lerp(minimumBounds, maximumBounds, (float)i / (float)(numberOfRaycasts - 1));
                }
                else
                {
                    switch (slopeCheck)
                    {
                        case SlopeCheck.Front:
                            if (myTransform.localScale.x > 0)
                            {
                                pos = (maximumBounds);
                            }
                            else
                            {
                                pos = (minimumBounds);
                            }
                            break;
                        case SlopeCheck.Back:
                            if (myTransform.localScale.x > 0)
                            {
                                pos = (minimumBounds);
                            }
                            else
                            {
                                pos = (maximumBounds);
                            }
                            break;
                    }
                }
                var hitRay = Physics2D.Raycast(pos + (Vector2)transform.position, direction, length, playerController.playerControllerData.terrainLayer);

                if (hitRay)
                {
                    switch (slopeCheck)
                    {
                        case SlopeCheck.Front:
                            Debug.DrawRay(pos + (Vector2)transform.position, direction * length, Color.green);
                            slopeAngleFront = Vector2.Angle(hitRay.normal, Vector2.up);
                            hit = CheckSlopeAngle(slopeCheck);
                            hitPointFront = hitRay.point;
                            return hit;
                        case SlopeCheck.Back:
                            Debug.DrawRay(pos + (Vector2)transform.position, direction * length, Color.green);
                            slopeAngleBack = Vector2.Angle(hitRay.normal, Vector2.up);
                            hit = CheckSlopeAngle(slopeCheck);
                            hitPointBack = hitRay.point;
                            return hit;
                        default:
                        case SlopeCheck.None:
                            break;
                    }
                    if(checkType == CheckType.Horizontal && slopeCheck == SlopeCheck.None)
                    {
                        touchWallAngle = Vector2.Angle(hitRay.normal, Vector2.up);
                    }
                        
#if UNITY_EDITOR
                    if (i == 0)
                    {
                        Debug.DrawRay(pos + (Vector2)transform.position, direction * length, Color.green);
                        hit = true;
                        continue;
                    }
                    Debug.DrawRay(pos + (Vector2)transform.position, direction * length, Color.red);
                    hit = true;
                    continue;
#endif
                    return true;
                }
#if UNITY_EDITOR
                Debug.DrawRay(pos + (Vector2)transform.position, direction * length, Color.blue);
#endif
            }
#if UNITY_EDITOR
            return hit;
#endif
            return false;
        } // End Touch Wall
    }
}