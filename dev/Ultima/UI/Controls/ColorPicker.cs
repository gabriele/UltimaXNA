﻿/***************************************************************************
 *   ColorPicker.cs
 *   
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 3 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UltimaXNA.Core.Graphics;
using UltimaXNA.Core.Input.Windows;
using UltimaXNA.Ultima.UI;

namespace UltimaXNA.Ultima.UI.Controls
{
    class ColorPicker : AControl
    {
        protected Texture2D m_huesTexture;
        protected Texture2D m_selectedIndicator;
        protected Rectangle m_openArea;

        protected int m_hueWidth, m_hueHeight;
        protected int[] m_hues;

        protected ColorPicker m_openColorPicker;

        bool m_getNewSelectedTexture;
        int m_index = 0;
        public int Index
        {
            get { return m_index; }
            set { m_index = value; m_getNewSelectedTexture = true; }
        }

        public int HueValue
        {
            get { return m_hues[Index]; }
            set
            {
                for (int i = 0; i < m_hues.Length; i++)
                {
                    if (value == m_hues[i])
                    {
                        Index = i;
                        break;
                    }
                }
            }
        }

        UserInterfaceService m_UserInterface;

        public ColorPicker(AControl owner, int page)
            : base(owner, page)
        {
            HandlesMouseInput = true;

            m_UserInterface = UltimaServices.GetService<UserInterfaceService>();
        }

        public ColorPicker(AControl owner, int page, Rectangle area, int swatchWidth, int swatchHeight, int[] hues)
            : this(owner, page)
        {
            m_isAnOpenSwatch = true;
            buildGumpling(area, swatchWidth, swatchHeight, hues);
        }

        public ColorPicker(AControl owner, int page, Rectangle closedArea, Rectangle openArea, int swatchWidth, int swatchHeight, int[] hues)
            : this(owner, page)
        {
            m_isAnOpenSwatch = false;
            m_openArea = openArea;
            buildGumpling(closedArea, swatchWidth, swatchHeight, hues);
        }

        void buildGumpling(Rectangle area, int swatchWidth, int swatchHeight, int[] hues)
        {
            m_hueWidth = swatchWidth;
            m_hueHeight = swatchHeight;
            Position = new Point(area.X, area.Y);
            Size = new Point(area.Width, area.Height);
            m_hues = hues;
            Index = 0;
            closeSwatch();
        }

        public override void Update(double totalMS, double frameMS)
        {
            if (m_isAnOpenSwatch)
            {
                if (m_huesTexture == null)
                {
                    m_huesTexture = IO.HuesXNA.HueSwatch(m_hueWidth, m_hueHeight, m_hues);
                m_selectedIndicator = IO.GumpData.GetGumpXNA(6000);
                }
            }
            else
            {
                if (m_huesTexture == null || m_getNewSelectedTexture)
                {
                    m_getNewSelectedTexture = false;
                    m_huesTexture = null;
                    m_huesTexture = IO.HuesXNA.HueSwatch(1, 1, new int[1] { m_hues[Index] });
                }
            }

            if (!m_isAnOpenSwatch)
            {
                if (m_isSwatchOpen && m_openColorPicker.IsInitialized)
                {
                    if (m_UserInterface.MouseOverControl != m_openColorPicker)
                        closeSwatch();
                }
            }

            base.Update(totalMS, frameMS);
        }

        public override void Draw(SpriteBatchUI spriteBatch, Point position)
        {
            if (m_isAnOpenSwatch)
            {
                spriteBatch.Draw2D(m_huesTexture, new Rectangle(position.X, position.Y, Width, Height), Vector3.Zero);
                spriteBatch.Draw2D(m_selectedIndicator, new Vector3(
                    (int)(X + (float)(Width / m_hueWidth) * ((Index % m_hueWidth) + 0.5f) - m_selectedIndicator.Width / 2),
                    (int)(Y + (float)(Height / m_hueHeight) * ((Index / m_hueWidth) + 0.5f) - m_selectedIndicator.Height / 2),
                    0), Vector3.Zero);
            }
            else
            {
                if (!m_isSwatchOpen)
                    spriteBatch.Draw2D(m_huesTexture, new Rectangle(position.X, position.Y, Width, Height), Vector3.Zero);
            }
            base.Draw(spriteBatch, position);
        }

        bool m_isAnOpenSwatch = false;
        bool m_isSwatchOpen = false;

        void openSwatch()
        {
            m_isSwatchOpen = true;
            if (m_openColorPicker != null)
            {
                m_openColorPicker.Dispose();
                m_openColorPicker = null;
            }
            m_openColorPicker = new ColorPicker(Owner, Page, m_openArea, m_hueWidth, m_hueHeight, m_hues);
            m_openColorPicker.MouseClickEvent = onOpenSwatchClick;
            ((Gump)Owner).AddControl(m_openColorPicker);
        }

        void closeSwatch()
        {
            m_isSwatchOpen = false;
            if (m_openColorPicker != null)
            {
                m_openColorPicker.Dispose();
                m_openColorPicker = null;
            }
        }

        protected override void OnMouseClick(int x, int y, MouseButton button)
        {
            if (!m_isAnOpenSwatch)
            {
                openSwatch();
            }
        }

        protected override void OnMouseOver(int x, int y)
        {
            if (m_isAnOpenSwatch)
            {
                int clickRow = x / (Width / m_hueWidth);
                int clickColumn = y / (Height / m_hueHeight);
                Index = clickRow + clickColumn * m_hueWidth;
            }
        }

        void onOpenSwatchClick(int x, int y, MouseButton button)
        {
            Index = m_openColorPicker.Index;
            closeSwatch();
        }
    }
}
