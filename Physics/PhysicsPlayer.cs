using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayerIOClient;

namespace EETuring.Physics
{
    public class PhysicsPlayer
    {   
        internal PhysicsWorld HostWorld { get; set; }

        //World constants
        internal const int Width = 16;
        internal const int Height = 16;
        internal const double portalMultiplier = 1.42;
        internal double GravityMultiplier { get { return HostWorld.WorldGravity; } }
        internal double SpeedMultiplier { get { return 1; } }

        private bool DoubleIsEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) < 0.00000001;
        }

        public PlayerState Tick(PlayerState s, Input input, int c)
        {
            PlayerState state = new PlayerState(s);

            for (int q = 0; q <= c; q++)
            {
                double reminderX;
                double currentSX;
                double osx;
                double ox;
                double tx;

                double reminderY;
                double currentSY;
                double osy;
                double oy;
                double ty;

                int cx = ((int)(state.X + 8) >> 4);
                int cy = ((int)(state.Y + 8) >> 4);

                state.current = HostWorld.GetBlock(cx, cy);
                state.onDot = state.current == 4;
                if (state.current == 4 || ItemId.IsClimbable(state.current))
                {
                    state.delayed = state.queue[1];
                    state.queue[0] = state.current;
                }
                else
                {
                    state.delayed = state.queue[0];
                    state.queue[0] = state.queue[1];
                }

                state.queue[1] = state.current;

                if (state.IsDead)
                {
                    state.Horizontal = 0;
                    state.Vertical = 0;
                }

                bool isGodMode = state.InGodMode;
                if (state.InGodMode)
                {
                    state.morx = 0;
                    state.mory = 0;
                    state.mox = 0;
                    state.moy = 0;
                }
                else
                {
                    switch (state.current)
                    {
                        case 1:
                            state.morx = -((int)state.gravity);
                            state.mory = 0;
                            break;
                        case 2:
                            state.morx = 0;
                            state.mory = -((int)state.gravity);
                            break;
                        case 3:
                            state.morx = (int)state.gravity;
                            state.mory = 0;
                            break;
                        case ItemId.SpeedLeft:
                        case ItemId.SpeedRight:
                        case ItemId.SpeedUp:
                        case ItemId.SpeedDown:
                        case ItemId.Chain:
                        case ItemId.NinjaLadder:
                        case ItemId.WineH:
                        case ItemId.WineV:
                        case 4:
                            state.morx = 0;
                            state.mory = 0;
                            break;
                        case ItemId.Water:
                            state.morx = 0;
                            state.mory = (int)PhysicsConfig.WaterBuoyancy;
                            break;
                        case ItemId.Mud:
                            state.morx = 0;
                            state.mory = (int)PhysicsConfig.MudBuoyancy;
                            break;
                        case ItemId.Fire:
                        case ItemId.Spike:
                            if (!state.IsDead && !state.isInvulnerable)
                            {
                                state.IsDead = true;
                            }
                            break;
                        default:
                            state.morx = 0;
                            state.mory = state.gravity;
                            break;
                    }

                    switch (state.delayed)
                    {
                        case 1:
                            state.mox = -state.gravity;
                            state.moy = 0;
                            break;
                        case 2:
                            state.mox = 0;
                            state.moy = -state.gravity;
                            break;
                        case 3:
                            state.mox = state.gravity;
                            state.moy = 0;
                            break;
                        case ItemId.SpeedLeft:
                        case ItemId.SpeedRight:
                        case ItemId.SpeedUp:
                        case ItemId.SpeedDown:
                        case ItemId.Chain:
                        case ItemId.NinjaLadder:
                        case ItemId.WineH:
                        case ItemId.WineV:
                        case 4:
                            state.mox = 0;
                            state.moy = 0;
                            break;
                        case ItemId.Water:
                            state.mox = 0;
                            state.moy = PhysicsConfig.WaterBuoyancy;
                            break;
                        case ItemId.Mud:
                            state.mox = 0;
                            state.moy = PhysicsConfig.MudBuoyancy;
                            break;
                        default:
                            state.mox = 0;
                            state.moy = state.gravity;
                            break;
                    }
                }

                if (state.moy == PhysicsConfig.WaterBuoyancy || state.moy == PhysicsConfig.MudBuoyancy)
                {
                    state.mx = state.Horizontal;
                    state.my = state.Vertical;
                }
                else
                {
                    if (state.moy != 0)
                    {
                        state.mx = state.Horizontal;
                        state.my = 0;
                    }
                    else
                    {
                        if (state.mox != 0)
                        {
                            state.mx = 0;
                            state.my = state.Vertical;
                        }
                        else
                        {
                            state.mx = state.Horizontal;
                            state.my = state.Vertical;
                        }
                    }
                }
                state.mx *= SpeedMultiplier;
                state.my *= SpeedMultiplier;
                state.mox *= GravityMultiplier;
                state.moy *= GravityMultiplier;

                state.ModifierX = (state.mox + state.mx);
                state.ModifierY = (state.moy + state.my);

                if (!DoubleIsEqual(state.speedX, 0) || state.modifierX != 0)
                {
                    state.speedX = (state.speedX + state.modifierX);
                    state.speedX = (state.speedX * PhysicsConfig.BaseDrag);
                    if ((state.mx == 0 && state.moy != 0) || (state.speedX < 0 && state.mx > 0) || (state.speedX > 0 && state.mx < 0) || (ItemId.IsClimbable(state.current) && !isGodMode))
                    {
                        state.speedX = (state.speedX * PhysicsConfig.NoModifierDrag);
                    }
                    else
                    {
                        if (state.current == ItemId.Water && !isGodMode)
                        {
                            state.speedX = (state.speedX * PhysicsConfig.WaterDrag);
                        }
                        else
                        {
                            if (state.current == ItemId.Mud && !isGodMode)
                            {
                                state.speedX = (state.speedX * PhysicsConfig.MudDrag);
                            }
                        }
                    }
                    if (state.speedX > 16)
                    {
                        state.speedX = 16;
                    }
                    else
                    {
                        if (state.speedX < -16)
                        {
                            state.speedX = -16;
                        }
                        else
                        {
                            if (state.speedX < 0.0001 && state.speedX > -0.0001)
                            {
                                state.speedX = 0;
                            }
                        }
                    }
                }
                if (!DoubleIsEqual(state.speedY, 0) || state.modifierY != 0)
                {
                    state.speedY = (state.speedY + state.modifierY);
                    state.speedY = (state.speedY * PhysicsConfig.BaseDrag);
                    if ((state.my == 0 && state.mox != 0) || (state.speedY < 0 && state.my > 0) || (state.speedY > 0 && state.my < 0) || (ItemId.IsClimbable(state.current) && !isGodMode))
                    {
                        state.speedY = (state.speedY * PhysicsConfig.NoModifierDrag);
                    }
                    else
                    {
                        if (state.current == ItemId.Water && !isGodMode)
                        {
                            state.speedY = (state.speedY * PhysicsConfig.WaterDrag);
                        }
                        else
                        {
                            if (state.current == ItemId.Mud && !isGodMode)
                            {
                                state.speedY = (state.speedY * PhysicsConfig.MudDrag);
                            }
                        }
                    }
                    if (state.speedY > 16)
                    {
                        state.speedY = 16;
                    }
                    else
                    {
                        if (state.speedY < -16)
                        {
                            state.speedY = -16;
                        }
                        else
                        {
                            if (state.speedY < 0.0001 && state.speedY > -0.0001)
                            {
                                state.speedY = 0;
                            }
                        }
                    }
                }
                if (!isGodMode)
                {
                    switch (state.current)
                    {
                        case ItemId.SpeedLeft:
                            state.speedX = -PhysicsConfig.Boost;
                            break;
                        case ItemId.SpeedRight:
                            state.speedX = PhysicsConfig.Boost;
                            break;
                        case ItemId.SpeedUp:
                            state.speedY = -PhysicsConfig.Boost;
                            break;
                        case ItemId.SpeedDown:
                            state.speedY = PhysicsConfig.Boost;
                            break;
                    }
                }

                reminderX = state.X % 1;
                currentSX = state.speedX;
                reminderY = state.Y % 1;
                currentSY = state.speedY;
                state.donex = false;
                state.doney = false;

                while ((currentSX != 0 && !state.donex) || (currentSY != 0 && !state.doney))
                {
                    #region processPortals()
                    state.current = HostWorld.GetBlock(cx, cy);
                    if (!isGodMode && (state.current == ItemId.Portal || state.current == ItemId.PortalInvisible))
                    {
                        if (state.lastPortal == null)
                        {
                            //OnHitPortal(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            state.lastPortal = new Point(cx, cy);
                            int[] data = HostWorld.GetBlockData(cx, cy);
                            if (data != null && data.Length == 3)
                            {
                                Point portalPoint = HostWorld.GetPortalById(data[2]);
                                if (portalPoint != null)
                                {
                                    int rot1 = HostWorld.GetBlockData(state.lastPortal.x, state.lastPortal.y)[0];
                                    int rot2 = HostWorld.GetBlockData(portalPoint.x, portalPoint.y)[0];
                                    if (rot1 < rot2)
                                    {
                                        rot1 += 4;
                                    }
                                    switch (rot1 - rot2)
                                    {
                                        case 1:
                                            state.SpeedX = state.SpeedY * portalMultiplier;
                                            state.SpeedY = -state.SpeedX * portalMultiplier;
                                            state.ModifierX = state.ModifierY * portalMultiplier;
                                            state.ModifierY = -state.ModifierX * portalMultiplier;
                                            reminderY = -reminderY;
                                            currentSY = -currentSY;
                                            break;
                                        case 2:
                                            state.SpeedX = -state.SpeedX * portalMultiplier;
                                            state.SpeedY = -state.SpeedY * portalMultiplier;
                                            state.ModifierX = -state.ModifierX * portalMultiplier;
                                            state.ModifierY = -state.ModifierY * portalMultiplier;
                                            reminderY = -reminderY;
                                            currentSY = -currentSY;
                                            reminderX = -reminderX;
                                            currentSX = -currentSX;
                                            break;
                                        case 3:
                                            state.SpeedX = -state.SpeedY * portalMultiplier;
                                            state.SpeedY = state.SpeedX * portalMultiplier;
                                            state.ModifierX = -state.ModifierY * portalMultiplier;
                                            state.ModifierY = state.ModifierX * portalMultiplier;
                                            reminderX = -reminderX;
                                            currentSX = -currentSX;
                                            break;
                                    }
                                    state.X = portalPoint.x * 16;
                                    state.Y = portalPoint.y * 16;
                                    state.lastPortal = portalPoint;
                                }
                            }
                        }
                    }
                    else
                    {
                        state.lastPortal = null;
                    }
                    #endregion

                    ox = state.X;
                    oy = state.Y;
                    osx = currentSX;
                    osy = currentSY;

                    #region stepX()
                    if (currentSX > 0)
                    {
                        if ((currentSX + reminderX) >= 1)
                        {
                            state.X += 1 - reminderX;
                            state.X = Math.Floor(state.X);
                            currentSX -= 1 - reminderX;
                            reminderX = 0;
                        }
                        else
                        {
                            state.X += currentSX;
                            currentSX = 0;
                        }
                    }
                    else
                    {
                        if (currentSX < 0)
                        {
                            if (!DoubleIsEqual(reminderX, 0) && (reminderX + currentSX) < 0)
                            {
                                currentSX += reminderX;
                                state.X -= reminderX;
                                state.X = Math.Floor(state.X);
                                reminderX = 1;
                            }
                            else
                            {
                                state.X += currentSX;
                                currentSX = 0;
                            }
                        }
                    }
                    if (HostWorld.Overlaps(state))
                    {
                        state.X = ox;
                        state.speedX = 0;
                        currentSX = osx;
                        state.donex = true;
                    }
                    #endregion

                    #region stepY()
                    if (currentSY > 0)
                    {
                        if ((currentSY + reminderY) >= 1)
                        {
                            state.Y += 1 - reminderY;
                            state.Y = Math.Floor(state.Y);
                            currentSY -= 1 - reminderY;
                            reminderY = 0;
                        }
                        else
                        {
                            state.Y += currentSY;
                            currentSY = 0;
                        }
                    }
                    else
                    {
                        if (currentSY < 0)
                        {
                            if (!DoubleIsEqual(reminderY, 0) && (reminderY + currentSY) < 0)
                            {
                                state.Y -= reminderY;
                                state.Y = Math.Floor(state.Y);
                                currentSY += reminderY;
                                reminderY = 1;
                            }
                            else
                            {
                                state.Y += currentSY;
                                currentSY = 0;
                            }
                        }
                    }
                    if (HostWorld.Overlaps(state))
                    {
                        state.Y = oy;
                        state.speedY = 0;
                        currentSY = osy;
                        state.doney = true;
                    }
                    #endregion
                }

                if (!state.IsDead)
                {
                    if ((input & Input.HoldSpace) != 0/* && !state.onDot*/)
                    {
                        if (state.SpeedX == 0 && !DoubleIsEqual(state.morx, 0) && !DoubleIsEqual(state.mox, 0) && state.X % 16 == 0)
                        {
                            state.SpeedX = (state.SpeedX - (state.morx * PhysicsConfig.JumpHeight));
                        }

                        if (state.SpeedY == 0 && !DoubleIsEqual(state.mory, 0) && !DoubleIsEqual(state.moy, 0) && state.Y % 16 == 0)
                        {
                            state.SpeedY = (state.SpeedY - (state.mory * PhysicsConfig.JumpHeight));
                        }
                    }

                    if (state.pastx != cx || state.pasty != cy)
                    {

                        // Might remove specific events soon, because you can make them now with void AddBlockEvent. (except OnGetCoin and OnGetBlueCoin)
                        switch (state.current)
                        {
                            case 100:   //coin
                                for (int i = 0; i < state.gotCoins.Count; i++)
                                {
                                    if (state.gotCoins[i].x == cx && state.gotCoins[i].y == cy)
                                    {
                                        goto found;
                                    }
                                }
                                state.gotCoins.Add(new Point(cx, cy));
                            found:
                                break;
                            case 101:   // bluecoin
                                for (int i = 0; i < state.gotBlueCoins.Count; i++)
                                {
                                    if (state.gotBlueCoins[i].x == cx && state.gotBlueCoins[i].y == cy)
                                    {
                                        goto found2;
                                    }
                                }
                                state.gotBlueCoins.Add(new Point(cx, cy));
                            found2:
                                break;
                            case 5:
                                // crown
                                break;
                            case 6:
                                // red key
                                break;
                            case 7:
                                // green key
                                break;
                            case 8:
                                // blue key
                                break;
                            case ItemId.SwitchPurple:
                                state.Purple = !state.Purple;
                                break;
                            case ItemId.Piano:
                                break;
                            case ItemId.Drum:
                                break;
                            case ItemId.Diamond:
                                break;
                            case ItemId.Cake:
                                break;
                            case ItemId.Checkpoint:
                                if (!isGodMode)
                                {
                                    state.LastCheckpointX = cx;
                                    state.LastCheckpointY = cy;
                                }
                                break;
                            case ItemId.BrickComplete:
                                break;
                        }
                        state.pastx = cx;
                        state.pasty = cy;
                    }
                }

                var imx = ((int)state.speedX << 8);
                var imy = ((int)state.speedY << 8);

                if (state.current != ItemId.Water && state.current != ItemId.Mud)
                {
                    if (imx == 0)
                    {
                        if (state.modifierX < 0.1 && state.modifierX > -0.1)
                        {
                            tx = (state.X % 16);
                            if (tx < 2)
                            {
                                if (tx < 0.2)
                                {
                                    state.X = Math.Floor(state.X);
                                }
                                else
                                {
                                    state.X -= tx / 15;
                                }
                            }
                            else
                            {
                                if (tx > 14)
                                {
                                    if (tx > 15.8)
                                    {
                                        state.X = Math.Ceiling(state.X);
                                    }
                                    else
                                    {
                                        state.X += (tx - 14) / 15;
                                    }
                                }
                            }
                        }
                    }

                    if (imy == 0)
                    {
                        if ((state.modifierY < 0.1) && (state.modifierY > -0.1))
                        {
                            ty = (state.Y % 16);
                            if (ty < 2)
                            {
                                if (ty < 0.2)
                                {
                                    state.Y = Math.Floor(state.Y);
                                }
                                else
                                {
                                    state.Y -= ty / 15;
                                }
                            }
                            else
                            {
                                if (ty > 14)
                                {
                                    if (ty > 15.8)
                                    {
                                        state.Y = Math.Ceiling(state.Y);
                                    }
                                    else
                                    {
                                        state.Y += (ty - 14) / 15;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return state;
        }

        public PhysicsPlayer(PhysicsWorld host)
        {
            HostWorld = host;
        }
    }
}
