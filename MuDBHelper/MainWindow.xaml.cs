﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace MuDBHelper
{
    public partial class MainWindow : Window
    {
        private DBItems current_item;
        private DBItemCategories current_category;
        private Dictionary<object, int> slot_indexes;
        private Dictionary<Button, Item> storage_items;
        private int current_item_index;
        private DBCharacter currentCharacter;
        private DBAccount currentAccount;
        private InventoryStorage currentInventory;

        public MainWindow()
        {
            //DBLoader.buildDBItems("Item.txt");
            //DBLoader.UpdateItemImages(false);

            InitializeComponent();
            fillCategories();
            fillLevelList();
            fillAddLevelList();
            initSlotIndexes();
            createInventoryGrid();
            displayExcOptions();

            storage_items = new Dictionary<Button, Item>();
        }

        private void fillCategories()
        {
            using (DBConnection conn = new DBConnection())
            {
                var categories = from e in conn.categories
                                 select e;
                items_list.ItemsSource = categories.ToList();
            }
        }

        private void fillLevelList()
        {
            List<string> levels = new List<string>();
            for(int i = 0; i < 16; i++)
                levels.Add("Level " + i);

            level_field.ItemsSource = levels;
            level_field.SelectedIndex = 0;
        }


        private void fillAddLevelList()
        {
            List<string> addLevels = new List<string>();
            for (int i = 0; i <= 28; i += 4)
                addLevels.Add("Level " + i);

            add_level_field.ItemsSource = addLevels;
            add_level_field.SelectedIndex = 0;
        }

        private void initSlotIndexes()
        {
            slot_indexes = new Dictionary<object, int>();
            slot_indexes.Add(slot_lweapon, 0);
            slot_indexes.Add(slot_rweapon, 1);
            slot_indexes.Add(slot_helm, 2);
            slot_indexes.Add(slot_chest, 3);
            slot_indexes.Add(slot_legs, 4);
            slot_indexes.Add(slot_gloves, 5);
            slot_indexes.Add(slot_boots, 6);
            slot_indexes.Add(slot_wings, 7);
            slot_indexes.Add(slot_pet, 8);
            slot_indexes.Add(slot_pen, 9);
            slot_indexes.Add(slot_lring, 10);
            slot_indexes.Add(slot_rring, 11);
        }

        public void getItemList(int category_id)
        {
            using (DBConnection conn = new DBConnection())
            {
                var items = from e in conn.items
                            where e.category_ID == category_id
                            select e;

                items_list.ItemsSource = items.ToList();
            }
        }


        private void ItemListOnSelect(object sender, SelectionChangedEventArgs e)
        {
            if (items_list.SelectedItem != null)
            {
                if (current_category == null)
                {
                    current_category =  (DBItemCategories)items_list.SelectedItems[0];
                    category_label.Content = current_category.name;
                    getItemList(current_category.ID);

                    displayExcOptions();

                    categoryBackButton.Visibility = Visibility.Visible;
                }

                else
                {                    
                    current_item = (DBItems)items_list.SelectedItem;
                    item_image_name_label.Content = current_item.name;
                    current_item_image.Source = new BitmapImage(new Uri(@"/images/items/" + current_item.image_path, UriKind.Relative));
                }
            }
           
            if(current_category == null) items_list.UnselectAll();           
        }

        private void CategoryBackOnClick(object sender, RoutedEventArgs e)
        {
            categoryBackButton.Visibility = Visibility.Hidden;
            category_label.Content = "Categories";
            current_category = null;
            fillCategories();
            showExcOptions(0);
        }

        private void showAccounts(string searchAccount)
        {
            using (DBConnection conn = new DBConnection())
            {
                var accounts = conn.accounts;

                if (searchAccount != null)
                    account_list.ItemsSource = accounts.Where(account => account.memb___id.Contains(searchAccount)).Select(account => account.memb___id);
                else
                    account_list.ItemsSource = accounts.Select(account => account.memb___id);
            }
        }

        private void showCharacters(string searchCharacter)
        {
            using (DBConnection conn = new DBConnection())
            {
                if (account_list.SelectedItem == null)
                {
                    var list = conn.characters;

                    if (searchCharacter != null)
                        character_list.ItemsSource = list.Where(character => character.Name.Contains(searchCharacter)).Select(character => character.Name);
                    else
                        character_list.ItemsSource = list.Select(character => character.Name);
                }

                else
                {
                    if (searchCharacter != null)
                    {
                        character_list.ItemsSource = conn.characters
                                                    .Where(character => character.AccountID == account_list.SelectedItem)
                                                    .Where(character => character.Name.Contains(searchCharacter))
                                                    .Select(character => character.Name);
                    }

                    else
                    {
                        character_list.ItemsSource = conn.characters
                            .Where(character => character.AccountID == account_list.SelectedItem)
                            .Select(character => character.Name);
                    }
                }
            }
        }

        private void AccountListOnOpen(object sender, EventArgs e)
        {
            showAccounts(null);
        }

        private void CharacterListOnOpen(object sender, EventArgs e)
        {
            showCharacters(null);
        }

        private void AccountSearchKeyUp(object sender, KeyEventArgs e)
        {
            showAccounts(account_list.Text);
        }

        private void CharacterSearchKeyUp(object sender, KeyEventArgs e)
        {
            showCharacters(character_list.Text);
        }

        private void showExcOptions(int type)
        {
            using (DBConnection conn = new DBConnection())
            {
                var opts = from e in conn.excOptions
                           where e.ID == type
                           select new 
                           { 
                               opt1 = e.option1, 
                               opt2 = e.option2,
                               opt3 = e.option3,
                               opt4 = e.option4,
                               opt5 = e.option5,
                               opt6 = e.option6
                           };

                var excOption = opts.First();
                exc_opt1.Content = excOption.opt1;
                exc_opt2.Content = excOption.opt2;
                exc_opt3.Content = excOption.opt3;
                exc_opt4.Content = excOption.opt4;
                exc_opt5.Content = excOption.opt5;
                exc_opt6.Content = excOption.opt6;
            }
        }

        private int getExcType(int category, int index)
        {
            if (category >= 0 && category < 6) return 1;

            else if (category >= 6 && category < 12) return 2;

            else return 0;
        }

        private void displayExcOptions()
        {
            if (current_category == null)
            {
                exc_opts_grid.Visibility = Visibility.Hidden;
                no_exc_label.Visibility = Visibility.Visible;
            }

            else
            {

                int excType = getExcType((int)current_category.ID, 0);

                if (excType != 0)
                {
                    no_exc_label.Visibility = Visibility.Hidden;
                    showExcOptions(excType);
                    exc_opts_grid.Visibility = Visibility.Visible;
                }
            }
        }

        private void displayRefineOptions()
        {
            if(current_item == null || current_item.itemType == null || current_item.itemType > 1)
            {
                ref_grid.Visibility = Visibility.Hidden;
                no_ref_label.Visibility = Visibility.Visible;
            }

            else
            {
                using(DBConnection conn = new DBConnection())
                {
                    var refineOpt = conn.refineOpts.Where(x => x.typeID == current_item.itemType).FirstOrDefault();
                    if (refineOpt != null)
                    {
                        ref_opt_check.Content = refineOpt;
                        if (refineOpt.option1 != null)
                            ref_opt1.Content = "+ " + refineOpt.option1;
                        else
                            ref_opt1.Visibility = Visibility.Hidden;

                        if (refineOpt.option2 != null)
                            ref_opt2.Content = "+ " + refineOpt.option2;
                        else
                            ref_opt2.Visibility = Visibility.Hidden;

                        no_ref_label.Visibility = Visibility.Hidden;
                        ref_grid.Visibility = Visibility.Visible;
                    }

                    else
                    {
                        ref_grid.Visibility = Visibility.Hidden; ;
                        no_ref_label.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void displayAncientOptions()
        {
            if (current_item == null || (current_item.set1 == -1 && current_item.set2 == -1 && current_item.set3 == -1))
            {
                anc_grid.Visibility = Visibility.Hidden;
                set_unavailable_label.Visibility = Visibility.Visible;                
            }

            else
            {
                using(DBConnection conn = new DBConnection())
                {
                    //Set one
                    //----------------------------------------------------------------------------------
                    if (current_item.set1 >= 0)
                    {
                        anc_set1.Content = conn.ancSets.FirstOrDefault(c => c.ID == current_item.set1);
                        anc_set1.Visibility = Visibility.Visible;
                    }

                    else anc_set1.Visibility = Visibility.Hidden;
                    //----------------------------------------------------------------------------------


                    //Set two
                    //----------------------------------------------------------------------------------
                    if (current_item.set2 >= 0)
                    {
                        anc_set2.Content = conn.ancSets.FirstOrDefault(c => c.ID == current_item.set2);
                        anc_set2.Visibility = Visibility.Visible;
                    }

                    else anc_set2.Visibility = Visibility.Hidden;
                    //----------------------------------------------------------------------------------


                    //Set three
                    //----------------------------------------------------------------------------------
                    if (current_item.set3 >= 0)
                    {
                        anc_set3.Content = conn.ancSets.FirstOrDefault(c => c.ID == current_item.set3);
                        anc_set3.Visibility = Visibility.Visible;
                    }

                    else anc_set3.Visibility = Visibility.Hidden;
                    //----------------------------------------------------------------------------------
                }
                
                set_unavailable_label.Visibility = Visibility.Hidden;
                anc_grid.Visibility = Visibility.Visible;
            }
        }


        private ExcOpts getExcOpts()
        {
            return new ExcOpts((bool) exc_opt1.IsChecked, (bool) exc_opt2.IsChecked, (bool) exc_opt3.IsChecked, 
                               (bool) exc_opt4.IsChecked, (bool) exc_opt5.IsChecked, (bool) exc_opt6.IsChecked);
        }

        private Item getItem()
        {
            if (current_item == null) return null;
            else
            {
                int category = (int) current_item.category_ID;
                int index = (int) current_item.ID;
                int level = level_field.SelectedIndex;
                int add_level = add_level_field.SelectedIndex;
                int dur = int.Parse(dur_field.Text);
                bool luck = (bool) luck_field.IsChecked;
                bool skill = (bool) skill_field.IsChecked;
                DBSets set = getSelectedAncient();
                int setID = (set != null)? (int) set.ID : -1;
                int harm_opt = ((DBHarmoneyOpts)harm_opt_list.SelectedValue).ID;
                int harm_lvl = (int) harm_opt_lvl.Value;
                int refID = ((DBRefineOpts)ref_opt_check.Content).ID;

                ExcOpts excOpts = getExcOpts();

                return new Item(index, category, skill, luck, level, add_level, dur, setID, harm_opt, harm_lvl, refID, excOpts);
            }
        }

        private void initBasicOptions(Item item)
        {
            level_field.SelectedIndex = item.level;
            add_level_field.SelectedIndex = item.addLevel;
            dur_field.Value = item.durability;
            luck_field.IsChecked = item.luck;
            skill_field.IsChecked = item.skill;
        }

        private void initExcOptions(Item item)
        {
            ExcOpts excOpts = item.excellent_options;
            exc_opt1.IsChecked = excOpts.isExcellent(0);
            exc_opt2.IsChecked = excOpts.isExcellent(1);
            exc_opt3.IsChecked = excOpts.isExcellent(2);
            exc_opt4.IsChecked = excOpts.isExcellent(3);
            exc_opt5.IsChecked = excOpts.isExcellent(4);
            exc_opt6.IsChecked = excOpts.isExcellent(5);
        }

        private void initItemOptions(Item item)
        {
            if (item == null) return;

            current_category = DBItemCategories.findCategory(item.category);
            current_item = DBItems.findItem(item.category, item.index);

            getItemList(item.category);
            items_list.SelectedIndex = item.index;
            items_list.ScrollIntoView(items_list.SelectedIndex);

            initBasicOptions(item);
            initExcOptions(item);
        }

        private DBSets getSelectedAncient()
        {
            foreach(RadioButton button in anc_grid.Children.Cast<RadioButton>())
            {
                if (button.IsChecked == true)
                    return (DBSets) button.Content;
            }

            return null;
        }

        private void changeSlotBackground(Uri path, Button slot)
        {
            if (path == null)
                slot.Background = Brushes.Transparent;
            else
            {
                ImageBrush iBrush = new ImageBrush();
                iBrush.ImageSource = new BitmapImage(path);
                slot.Background = iBrush;
            }
        }

        private void CharacterSlotOnClick(object sender, RoutedEventArgs e)
        {
            if (current_item == null) return;

            changeSlotBackground(new Uri(current_item_image.Source.ToString()), (Button)sender);

            int slotIndex = slot_indexes[sender];

            updateSpace(slotIndex, currentInventory.character);
        }

        private void updateSpace(int index, StorageSpace space)
        {
            space.saveItem(getItem(), index);
            currentInventory.saveCharacterInventory(currentCharacter.Name); 
        }

        private void removeFromSpace(int index, StorageSpace space)
        {
            space.removeItem(index);
            currentInventory.saveCharacterInventory(currentCharacter.Name);
            initInventoryDisplay();
        }

        private void createInventoryGrid()
        {
            createGrid(8, 8, inventory_grid);
        }

        private void createVaultGrid()
        {
            createGrid(15, 8, vault_storage_container);
        }

        private void createStoreGrid()
        {
            createGrid(4, 8, store_storage_container);
        }

        private void createExtraInventoryGrid()
        {
            createGrid(8, 8, e_inv_storage_container);
        }

        private void displayStorageContainer(Grid container)
        {
            foreach(var other in storage_containers.Children.Cast<UIElement>().Where(x => x != container))            
                other.Visibility = Visibility.Hidden;

            container.Visibility = Visibility.Visible; 
        }

        private void createGrid(int numRows, int numCols, Grid container)
        {            
            int colWidth = 35;
            int rowHeight = 35;

            for (int row = 0; row < numRows; row++)
            {
                GridLength gridLength = new GridLength(1, GridUnitType.Auto);
                RowDefinition rowDef = new RowDefinition();
                rowDef.Height = gridLength;
                container.RowDefinitions.Add(rowDef);

                ColumnDefinition colDef = new ColumnDefinition();
                colDef.Width = gridLength;
                container.ColumnDefinitions.Add(colDef);

                for (int col = 0; col < numCols; col++)
                {
                    int leftThick = 1;
                    int topThick = 1;

                    if (col > 0)
                        leftThick = 0;

                    if (row > 0)
                        topThick = 0;

                    Button inventorySlot = new Button();
                    inventorySlot.BorderThickness = new Thickness(leftThick, topThick, 1, 1);
                    inventorySlot.BorderBrush = Brushes.Black;
                    inventorySlot.HorizontalAlignment = HorizontalAlignment.Left;
                    inventorySlot.VerticalAlignment = VerticalAlignment.Top;
                    inventorySlot.Width = colWidth;
                    inventorySlot.Height = rowHeight;

                    inventorySlot.Background = Brushes.Transparent;
                    inventorySlot.Click += new RoutedEventHandler(inventorySlotOnClick);

                    container.Children.Add(inventorySlot);
                    Grid.SetRow(inventorySlot, row);
                    Grid.SetColumn(inventorySlot, col);
                }
            }
        }

        private void initCharacterDisplay()
        {
            CharacterSpace characterItems = currentInventory.character;
            
            foreach(var slot in slot_indexes)
            {
                Button slotButton = (Button) slot.Key;
                int index = slot.Value;
                Item item = characterItems.items[index];

                if (!item.isItemEmpty())
                {
                    string imagePath = item.getImagePath();
                    changeSlotBackground(new Uri(@"images/items/" + imagePath, UriKind.Relative), slotButton);
                }

                else
                    changeSlotBackground(null, slotButton);
            }
        }

        private void initCharacter(string characterName)
        {
            using(DBConnection conn = new DBConnection())
            {
                currentCharacter = conn.characters.Single(c => c.Name == characterName);
                string hex = BitConverter.ToString(currentCharacter.Inventory).Replace("-", "");
                currentInventory = new InventoryStorage(hex);
                initCharacterDisplay();
                initInventoryDisplay();
            }
        }

        private void resetInventory()
        {
            storage_items.Clear();

            foreach (Button current in inventory_grid.Children.Cast<UIElement>())
            {
                Grid.SetRowSpan(current, 1);
                Grid.SetColumnSpan(current, 1);
                current.Width = 35;
                current.Height = 35;
                current.Visibility = Visibility.Visible;
            }
        }

        private bool isInsideBoundaries(DBItems item, Button slot)
        {
            int col = Grid.GetColumn((Button)slot);
            int row = Grid.GetRow((Button)slot);
            int width = (int)item.width - 1;
            int height = (int)item.height - 1;

            if ((col + width) >= inventory_grid.ColumnDefinitions.Count || (row + height) >= inventory_grid.RowDefinitions.Count)
                return false;
            else
                return true;
        }

        private void displayItemInInventory(DBItems item, Button slot)
        {
            int col = Grid.GetColumn((Button)slot);
            int row = Grid.GetRow((Button)slot);
            int width = (int)item.width - 1;
            int height = (int)item.height - 1;

            int cellDim = 35;

            if (width > 0 || height > 0)
            {
                foreach (Button current in inventory_grid.Children.Cast<UIElement>()
                    .Where(c => (Grid.GetColumn(c) >= col && Grid.GetColumn(c) <= col + width)
                    && (Grid.GetRow(c) >= row && Grid.GetRow(c) <= row + height)))
                {
                    current.Visibility = Visibility.Hidden;
                }

                ((Button)slot).Visibility = Visibility.Visible;
            }

            ((Button)slot).Width = (width + 1) * cellDim;
            ((Button)slot).Height = (height + 1) * cellDim;
            Grid.SetColumnSpan((Button)slot, width + 1);
            Grid.SetRowSpan((Button)slot, height + 1);

            changeSlotBackground(new Uri(@"images/items/" + item.image_path, UriKind.Relative), (Button)slot);
        }

        public void inventorySlotOnClick(object sender, RoutedEventArgs e)
        {
            int numCols = inventory_grid.ColumnDefinitions.Count;
            int index = (Grid.GetRow((Button) sender) * numCols) + Grid.GetColumn((Button) sender);

            if (storage_items.ContainsKey((Button)sender))
            {
                current_item_index = index;
                initItemOptions(storage_items[(Button)sender]);
                storage_ctrls_container.Visibility = Visibility.Visible;
                displaySelectedTabOptions();
                category_label.Content = current_category.name;
                categoryBackButton.Visibility = Visibility.Visible;

                foreach(Button btn in inventory_grid.Children.Cast<UIElement>().Where(c => Grid.GetColumn(c) > 0 && Grid.GetRow(c) > 0))
                {
                    btn.BorderThickness = new Thickness(0, 0, 1, 1);
                    btn.BorderBrush = Brushes.Black;
                }

                ((Button)sender).BorderThickness = new Thickness(1, 1, 1, 1);
                ((Button)sender).BorderBrush = Brushes.White;
            }

            else
            {
                if (current_item != null && isInsideBoundaries(current_item, (Button)sender))
                {
                    storage_items.Add((Button)sender, getItem());
                    displayItemInInventory(current_item, (Button)sender);                                        
                    updateSpace(index, currentInventory.inventory);
                }
            }
        }

        private void initInventoryDisplay()
        {
            if (currentInventory == null) return;

            InventorySpace inventory = currentInventory.inventory;
            int storageIndex = 0;
            resetInventory();

            foreach(Button slot in inventory_grid.Children.Cast<UIElement>())
            {
                Item current = inventory.items[storageIndex];
                storageIndex++;

                if (current.isItemEmpty() || slot.Visibility == Visibility.Hidden)
                {
                    changeSlotBackground(null, slot);
                    continue;
                }

                DBItems storedItem = DBItems.findItem(current.category, current.index);

                if (storedItem != null)
                {                    
                    storage_items.Add(slot, current);
                    displayItemInInventory(storedItem, slot);
                }
            }            
        }

        private void showHarmoneyOptions()
        {
            if(current_category == null || current_category.ID > 11)
            {
                harm_grid.Visibility = Visibility.Hidden;
                no_harm_label.Visibility = Visibility.Visible;
            }

            else
            {
                int type;
                if (current_category.ID < 7)
                    type = 1;

                else
                    type = 2;

                using(DBConnection conn = new DBConnection())
                {
                    harm_opt_list.ItemsSource = conn.harmOpts.Where(h => h.type == type);                   
                }

                no_harm_label.Visibility = Visibility.Hidden;
                harm_grid.Visibility = Visibility.Visible;                
            }
        }

        private void CharacterListOnSelect(object sender, SelectionChangedEventArgs e)
        {
            string usernameSelected = (string) character_list.SelectedItem;
            initCharacter(usernameSelected);
        }
        

        private void OnStorageSaveClick(object sender, RoutedEventArgs e)
        {
            updateSpace(current_item_index, currentInventory.inventory);
        }

        private void OnStorageRemoveClick(object sender, RoutedEventArgs e)
        {
            removeFromSpace(current_item_index, currentInventory.inventory);
            storage_ctrls_container.Visibility = Visibility.Hidden;
        }

        private void displaySelectedTabOptions()
        {
            if (tab_anc.IsSelected)
                displayAncientOptions();

            else if (tab_harm.IsSelected)
                showHarmoneyOptions();

            else if (tab_ref.IsSelected)
                displayRefineOptions();
        }

        private void OnAdvancedPropertiesChanged(object sender, SelectionChangedEventArgs e)
        {
            displaySelectedTabOptions();
        }

        private void OnStorageChange(object sender, SelectionChangedEventArgs e)
        {
            if (storage_containers == null) return;

            switch(storage_list.SelectedIndex)
            {
                case 0:
                    displayStorageContainer(character_storage_container);                    
                    break;
                case 1:
                    displayStorageContainer(inventory_storage_container);
                    createInventoryGrid();
                    break;
                case 2:
                    displayStorageContainer(vault_storage_container);
                    createVaultGrid();
                    break;
                case 3:
                    displayStorageContainer(store_storage_container);
                    createStoreGrid();
                    break;
                case 4:
                    displayStorageContainer(e_inv_storage_container);
                    createExtraInventoryGrid();
                    break;                    
            }
        }
    }
}
