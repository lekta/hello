using System.Collections.Generic;
using LH.Domain;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    public class DevWindow : EditorWindow {
        private const int HEADER_SPACING = 7;
        private const int HEADER_BUTTONS_MARGIN = 2;
        private const int TAB_BUTTON_IN_A_ROW = 5;

        private static readonly List<DevWindowPage> _pages = new() {
            new DevPage_Gameplay(),
            new DevPage_CI(),
            null,
            null,
            null
        };
        private static DevWindowPage _currentPage;

        private static readonly DevGuiScroll _frameScroll = new();


        [MenuItem("Tools/Dev Window %`", false, 3000)]
        private static void ShowDevelopWindow() {
            ShowAndFocus();
        }

        private static void ShowAndFocus() {
            var window = GetWindow<DevWindow>();
            window.ShowUtility();
            window.Focus();
        }

        private void OnEnable() {
            titleContent = new GUIContent("Dev`");

            if (_currentPage == null) {
                SetPage(_pages[0]);
            }
        }


        private void OnGUI() {
            GuiHeader();
            GuiCurrentPage();
        }

        private void GuiHeader() {
            using (new GUILayout.HorizontalScope()) {
                GUILayout.Space(HEADER_SPACING);

                for (int i = 0; i < 5; i++) {
                    ToolbarPageSelect(i);
                }
            }

            DevGui.HorizontalLine();
            GUILayout.Space(4);
        }

        private void ToolbarPageSelect(int pageIdx) {
            var page = _pages.GetAtOrDefault(pageIdx);
            if (page == null) {
                return;
            }
            var buttonLabel = page.Label;
            var buttonWidth = GUILayout.Width(GetHeaderButtonWidth());

            bool isActive = _currentPage == page;
            bool newIsActive = GUILayout.Toggle(isActive, buttonLabel, EditorStyles.toolbarButton, buttonWidth);

            if (newIsActive != isActive) {
                GUIUtility.keyboardControl = 0;
            }
            if (!isActive && newIsActive) {
                SetPage(page);
            }

            GUILayout.Space(HEADER_BUTTONS_MARGIN);
        }

        private float GetHeaderButtonWidth() {
            // window size without borders and space, divided by 5 elements in row
            return (position.width - HEADER_SPACING * 3) / TAB_BUTTON_IN_A_ROW - HEADER_BUTTONS_MARGIN;
        }

        private void SetPage(DevWindowPage page) {
            _currentPage = page;
            titleContent = new GUIContent($"D: {page.Label}");
        }

        private void GuiCurrentPage() {
            if (_currentPage == null) {
                return;
            }
            _frameScroll.Draw(_currentPage.Gui);
        }
    }
}