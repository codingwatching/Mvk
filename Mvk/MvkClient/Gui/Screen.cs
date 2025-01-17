﻿using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;
using System;
using System.Collections.Generic;

namespace MvkClient.Gui
{
    /// <summary>
    /// Абстрактный класс скрина для заменяющих скринов, с откликами мыши и клавиатуры
    /// с одним графическим листом
    /// </summary>
    public abstract class Screen : ScreenBase, IDisposable
    {
        /// <summary>
        /// Колекция всех контролов
        /// </summary>
        public List<Control> Controls { get; protected set; } = new List<Control>();
        /// <summary>
        /// Координата мыши
        /// </summary>
        public vec2i MouseCoord { get; protected set; }
        /// <summary>
        /// Для меню фпс 20, в игре обычный игровой
        /// </summary>
        public bool IsFpsMin { get; protected set; } = true;
        
        /// <summary>
        /// Откуда зашёл
        /// </summary>
        protected EnumScreenKey where;
        /// <summary>
        /// Тип фона
        /// </summary>
        protected EnumBackground background = EnumBackground.Menu;
        /// <summary>
        /// Графический лист
        /// </summary>
        protected uint dList;
        /// <summary>
        /// флаг, для принудительного рендера
        /// </summary>
        protected bool isRender = false;
        /// <summary>
        /// Строка подсказки
        /// </summary>
        private string toolTip = "";

        public Screen(Client client) : base(client) { }

        /// <summary>
        /// Инициализация нажатие кнопки
        /// </summary>
        protected void InitButtonClick(Button button)
        {
            if (button.ScreenKey != EnumScreenKey.None)
            {
                button.Click += (sender, e) => OnFinished(button.ScreenKey);
            }
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public override void Resized()
        {
            base.Resized();
            foreach (Control control in Controls)
            {
                control.Resized();
            }
            ResizedScreen();
            RenderList();
        }

        /// <summary>
        /// Такт игрового времени
        /// </summary>
        public override void Tick()
        {
            if (!IsFpsMin)
            {
                foreach (Control control in Controls)
                {
                    if (control is TextBox textBox && textBox.Focus)
                    {
                        textBox.UpdateCursorCounterTick();
                    }
                }
            }
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected virtual void ResizedScreen() { }

        /// <summary>
        /// Прорисовка
        /// </summary>
        public void Draw(float timeIndex)
        {
            if (isRender)
            {
                isRender = false;
                RenderList();
            }
            foreach (Control control in Controls)
            {
                if (control is TextBox textBox && textBox.Focus)
                {
                    if (IsFpsMin) textBox.UpdateCursorCounterTick();
                    textBox.CursorCounterDraw();
                    if (textBox.IsRender) RenderList();
                }
            }
            GLRender.ListCall(dList);
            DrawAdd(timeIndex);
        }

        /// <summary>
        /// Дополнительная прорисовка сверх основной
        /// </summary>
        protected virtual void DrawAdd(float timeIndex)
        {
            if (toolTip != "")
            {
                DrawHoveringText(MouseCoord.x, MouseCoord.y);
            }
        }

        /// <summary>
        /// Рендер листа
        /// </summary>
        protected void RenderList()
        {
            uint list = GLRender.ListBegin();
            Ortho2D();
            Render();
            gl.EndList();
            GLRender.ListDelete(dList);
            dList = list;
        }

        /// <summary>
        /// Контролы
        /// </summary>
        protected virtual void RenderControls()
        {
            foreach(Control control in Controls)
            {
                GLRender.PushMatrix();
                GLRender.Translate(control.Position.x, control.Position.y, 0);
                GLRender.Scale(SizeInterface, SizeInterface, 1);
                control.Render();
                GLRender.PopMatrix();
            }
        }

        /// <summary>
        /// Окно
        /// </summary>
        protected virtual void RenderWindow() { }

        /// <summary>
        /// Фон
        /// </summary>
        protected void RenderBackground()
        {
            if (background == EnumBackground.Menu)
            {
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                GLWindow.Texture.BindTexture(AssetsTexture.OptionsBackground);
                gl.Color(.4f, .4f, .4f, 1f);
                GLRender.Rectangle(0, 0, Width, Height, 0, 0, Width / 32f, Height / 32f);
            }
            else if (background == EnumBackground.TitleMain)
            {
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                GLWindow.Texture.BindTexture(AssetsTexture.Title);
                gl.Color(1.0f, 1.0f, 1.0f, 1f);
                float k = Height * 2f / (float)Width;
                if (k > 2f)
                {
                    GLRender.Rectangle(0, 0, Width, Height, 0.5f, 0, 1.0f, 0.5f);
                }
                else if (k > 1f)
                {
                    k = (k - 1.0f) / 2f;
                    GLRender.Rectangle(0, 0, Width, Height, k, 0, 1.0f, 0.5f);
                }
                else
                {
                    GLRender.Rectangle(Width - Height, 0, Width, Height, 0.5f, 0, 1.0f, 0.5f);
                    GLRender.Rectangle(0, 0, Width - Height, Height, 0f, 0, 0.5f, 0.5f);
                }
                int w = Width < 1500 ? Width / 2 : 1024;
                int h = Width < 1500 ? Width / 8 : 256;
                GLRender.Rectangle(0, 0, w, h, 0f, 0.5f, 1f, 0.75f);
                GLWindow.Texture.BindTexture(AssetsTexture.Font8);
                string text = ClientMain.NameVersion;
                vec3 color = new vec3(1);
                GLRender.PushMatrix();
                GLRender.Scale(SizeInterface, SizeInterface, 1);
                FontRenderer.RenderString(10, Height / SizeInterface - 20, text, FontSize.Font8, color, 1, true, .2f, 1);
                text = "SuperAnt";
                int ws = FontRenderer.WidthString(text, FontSize.Font8) + 10;
                FontRenderer.RenderString(Width / SizeInterface - ws, Height / SizeInterface - 20, text, FontSize.Font8, color, 1, true, .2f, 1);
                GLRender.PopMatrix();
            }
            else if (background == EnumBackground.Game)
            {
                return;
            }
            else
            {
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                vec4 colBg;
                if (background == EnumBackground.GameOver) colBg = new vec4(.4f, .1f, .1f, .5f);
                else if (background == EnumBackground.Loading) colBg = new vec4(1f, 1f, 1f, 1f);
                else colBg = new vec4(.3f, .3f, .3f, .5f);
                GLRender.Rectangle(0, 0, Width, Height, colBg);
            }
        }

        /// <summary>
        /// Прорисовка
        /// </summary>
        protected virtual void Render()
        {
            // Фон
            RenderBackground();
            // Окно если оно есть
            RenderWindow();
            // Контролы
            RenderControls();
        }

        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public virtual void MouseMove(int x, int y)
        {
            MouseCoord = new vec2i(x, y);
            bool isRender = false;
            toolTip = "";
            string toolTipCache = "";
            foreach (Control control in Controls)
            {
                toolTipCache = control.GetToolTip();
                if (toolTipCache != "") toolTip = toolTipCache;
                if (control.Visible && control.Enabled) control.MouseMove(x, y);
                if (control.IsRender) isRender = true;
            }

            if (isRender) RenderList();
        }

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public virtual void MouseDown(MouseButton button, int x, int y) => MouseUpDown(button, x, y, true);
        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public void MouseUp(MouseButton button, int x, int y) => MouseUpDown(button, x, y, false);

        protected void MouseUpDown(MouseButton button, int x, int y, bool isDown)
        {
            foreach (Control control in Controls)
            {
                if (control.Visible && control.Enabled)
                {
                    if (isDown) control.MouseDown(button, x, y);
                    else control.MouseUp(button, x, y);
                }
            }
        }

        /// <summary>
        /// Вращение колёсика мыши
        /// </summary>
        /// <param name="delta">смещение</param>
        public virtual void MouseWheel(int delta, int x, int y) { }

        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        public void KeyPress(char key)
        {
            foreach (Control control in Controls)
            {
                if (control.Visible && control.Enabled && control.Focus)
                {
                    control.KeyPress(key);
                    break;
                }
            }
        }

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        public virtual void KeyDown(int key) { }

        public void Dispose() => Delete();
        public void Delete()
        {
            OnFinishing();
            GLRender.ListDelete(dList);
        }

        public void AddControls(Control control)
        {
            control.Init(this);
            Controls.Add(control);
        }

        /// <summary>
        /// Происходит перед закрытием окна
        /// </summary>
        protected virtual void OnFinishing() { }

        /// <summary>
        /// Закончен скрин
        /// </summary>
        public event ScreenEventHandler Finished;
        protected virtual void OnFinished(EnumScreenKey key) => OnFinished(new ScreenEventArgs(key));
        protected virtual void OnFinished(ScreenEventArgs e) => Finished?.Invoke(this, e);

        #region ToolTip

        /// <summary>
        /// Нарисовать всплывающий текст
        /// </summary>
        protected void DrawHoveringText(int x, int y)
        {
            string[] stringSeparators = new string[] { "\r\n" };
            string[] strs = toolTip.Split(stringSeparators, StringSplitOptions.None);
            int h = 0;
            int w = 0;
            int wMax = 0;
            foreach (string str in strs)
            {
                w = FontRenderer.WidthString(str, FontSize.Font12);
                if (w > wMax) wMax = w;
                h += FontAdvance.VertAdvance[(int)FontSize.Font12] + 4;
            }
            h += 4;
            w = wMax + 10;
            vec4 colorBorder = new vec4(.89f, .78f, .66f, 1f);
            GLRender.PushMatrix();
            GLRender.Texture2DDisable();
            GLRender.Translate(x - 16, y + 32, 0);
            GLRender.Scale(SizeInterface, SizeInterface, 1);
            GLRender.Rectangle(1, 0, w, 1, colorBorder);
            GLRender.Rectangle(1, h, w, h + 1, colorBorder);
            GLRender.Rectangle(0, 1, 1, h, colorBorder);
            GLRender.Rectangle(w, 1, w + 1, h, colorBorder);
            GLRender.Rectangle(1, 1, w, h, new vec4(.31f, .26f, .21f, .9f));
            GLRender.Texture2DEnable();
            GLWindow.Texture.BindTexture(AssetsTexture.Font12);
            FontRenderer.RenderText(6, 6, toolTip, FontSize.Font12, new vec3(1), 1, true, .2f, 1);
            GLRender.PopMatrix();
        }

        #endregion
    }
}
