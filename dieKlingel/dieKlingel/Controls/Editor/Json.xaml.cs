using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Acr.UserDialogs;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace dieKlingel.Controls.Editor
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Json : ContentView, IBase
    {
        private Expander mainExpander = new Expander();
        public Json()
        {
            InitializeComponent();
        }
        public EditorType Type { get { return EditorType.Json; } }

        private string text = "";

        public string Text { 
            get 
            {
                return CreateJson();
            } 
            set 
            { 
                InitialzeView(value);
                text = value;
            } 
        }
        public event EventHandler<CustomCommandEventArgs> CustomCommand;

        private void InitialzeView(string plaintext)
        {
            mainExpander.Header = new Label
            {
                Text = "Benutzer"
            };

            JToken token = JToken.Parse(plaintext);
            if(token.Type == JTokenType.Array)
            {
                CreateArrayExpander(JArray.Parse(plaintext), stack_layout, "" ,false);
            }else 
            if(token.Type == JTokenType.Object)
            {
                CreateObjectExpander(JObject.Parse(plaintext), stack_layout, "", false);
            }
        }

        private void CreateArrayExpander(JArray array, StackLayout view = null, string title = "", bool expandable = true, int index = 0)
        {
            // Crete a new Expander for an Json Object of Type Array;
            Expander expander = new Expander();
            // Set the Expand Icon
            expander.Header = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            Image icon = new Image
            {
                Source = "arrow_expand_up",
                WidthRequest = 18,
                HeightRequest = 18,
                Rotation = 90,
                BindingContext = expander
            };
            icon.SetBinding(Image.RotationProperty, "IsExpanded", BindingMode.Default, new ExpanderIconRotationConverter());

            (expander.Header as StackLayout).Children.Add(icon);
            // add Entry if tile for entry is given
            if (!string.IsNullOrEmpty(title))
            {
                ((StackLayout)expander.Header).Children.Add(new Entry
                {
                    FontSize = 18,
                    Text = title,
                    WidthRequest = 100
                });
            }
            // add delete button
            ImageButton DeleteButton = new ImageButton
            {
                Source = "delete",
                WidthRequest = 24,
                HeightRequest = 24,
                HorizontalOptions = LayoutOptions.EndAndExpand,
            };
            DeleteButton.Clicked += DeleteButton_Clicked;
            ((StackLayout)expander.Header).Children.Add(DeleteButton);

            // Create content Layout
            StackLayout content = new StackLayout();
            JType.SetJtype(content, JType.Type.Array);
            /// move 15 pixel to the right for hirachy view
            content.Margin = expandable ? new Thickness(15, 0, 0, 0) : new Thickness(0, 0, 0, 0);

            // iterate through every item
            foreach(JToken token in array)
            {
                if(token.Type ==  JTokenType.Array)
                {
                    CreateArrayExpander(JArray.Parse(token.ToString()), content);
                } else
                if (token.Type == JTokenType.Object)
                {
                    CreateObjectExpander(JObject.Parse(token.ToString()), content);
                }
                else
                {
                    CreatePropertyExpander(new JProperty("", token.ToString()), content);
                }
            }

            // Add an 'add' Button to the Current Layout
            ImageButton button = new ImageButton
            {
                Source = "add",
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = 18
            };
            button.Clicked += AddButton_Clicked;
            content.Children.Add(button);

            // Set Expander Content
            expander.Content = content;
            // Add the Expander to the parent view - for recursive calls
            view?.Children.Insert((int)view?.Children.Count - index, expandable ? expander : (View)content);
        }

        private void CreateObjectExpander(JObject obj, StackLayout view = null, string title = "", bool expandable = true, int index = 0) 
        {
            // Crete a new Expander for an Json Object of Type Array;
            Expander expander = new Expander();
            expander.Header = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            // set the expand icon
            Image icon = new Image
            {
                Source = "arrow_expand_up",
                WidthRequest = 18,
                HeightRequest = 18,
                Rotation = 90,
                BindingContext = expander
            };
            icon.SetBinding(Image.RotationProperty, "IsExpanded", BindingMode.Default, new ExpanderIconRotationConverter());
            ((StackLayout)expander.Header).Children.Add(icon);

            //  // add Entry if tile for entry is given
            if (!string.IsNullOrEmpty(title))
            {
                ((StackLayout)expander.Header).Children.Add(new Entry
                {
                    FontSize = 18,
                    Text = title,
                    WidthRequest = 100
                });
            }

            ImageButton DeleteButton = new ImageButton
            {
                Source = "delete",
                WidthRequest = 24,
                HeightRequest = 24,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Color.Transparent
            };
            DeleteButton.Clicked += DeleteButton_Clicked;

            ((StackLayout)expander.Header).Children.Add(DeleteButton);

            StackLayout content = new StackLayout();
            content.Margin = new Thickness(15, 0, 0, 0);
            JType.SetJtype(content, JType.Type.Object);

            foreach (JProperty prop in obj.Properties())
            {
                if(prop.Value is JObject)
                {
                    //System.Diagnostics.Debug.WriteLine(prop.Name);
                    CreateObjectExpander((JObject)prop.Value, content, prop.Name);
                }else 
                if(prop.Value is JArray)
                {
                    CreateArrayExpander((JArray)prop.Value, content);
                }else 
                {
                    CreatePropertyExpander(prop, content, prop.Name);
                }
            }

            ImageButton AddButton = new ImageButton
            {
                Source = "add",
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = 18
            };
            AddButton.Clicked += AddButton_Clicked;
            content.Children.Add(AddButton);
                
            expander.Content = content;
            view?.Children.Insert((int)view?.Children.Count - index, expandable ? (View)expander : (View)content);
        }


        private void CreatePropertyExpander(JProperty property, StackLayout view = null, string title = "", int index  = 0)
        {
            //System.Diagnostics.Debug.WriteLine(property.Name + " - " + property.Value);
            StackLayout line = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            JType.SetJtype(line, JType.Type.Property);

            if (!string.IsNullOrEmpty(title))
            {
                line.Children.Add(new Entry
                {
                    FontSize = 18,
                    Text = title,
                    WidthRequest = 100
                }) ;
            }
            line.Children.Add(new Entry
            {
                FontSize = 18,
                Text = property.Value.ToString(),
                HorizontalOptions = LayoutOptions.FillAndExpand
            });
            ImageButton DeleteButton = new ImageButton
            {
                Source = "delete",
                WidthRequest = 24,
                HeightRequest = 24,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Color.Transparent
            };
            DeleteButton.Clicked += DeleteButton_Clicked;
            line.Children.Add(DeleteButton);
            view?.Children.Insert((int)view?.Children.Count - index , line);
        }

        private async void AddButton_Clicked(object sender, EventArgs e)
        {
            ImageButton button = sender as ImageButton;
            StackLayout layout = button.Parent as StackLayout;

            string title = "";
            switch (JType.GetJType(layout))
            {
                case JType.Type.Array:
                    title = "";
                    break;
                case JType.Type.Property:
                case JType.Type.Object:
                    title = "Key";
                    break;
            }

            string res = await UserDialogs.Instance.ActionSheetAsync("Hinzufügen", "Abbrechen", null, System.Threading.CancellationToken.None, new string[] { "Property", "Array", "Object" });
            switch (res)
            {
                case "Property":
                    CreatePropertyExpander(new JProperty("Key", "Value"), layout, title, 1);
                    break;
                case "Array":
                    CreateArrayExpander(new JArray(), layout, title, true, 1);
                    break;
                case "Object":
                    CreateObjectExpander(new JObject(), layout, title, true, 1);
                    break;
            }
            if (layout.Parent.Parent.Parent is Expander)
            {
                (layout.Parent.Parent.Parent as Expander).ForceUpdateSize();
            }
        }

        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            bool result = await UserDialogs.Instance.ConfirmAsync("Bist du dir Sicher ?", "Löschen", "Ja", "Nein");
            if (result)
            {
                Expander parentExpander = null;
                if((sender as ImageButton).Parent.Parent.Parent is Expander) 
                {
                    parentExpander = ((sender as ImageButton).Parent.Parent.Parent as Expander);
                }else 
                if((sender as ImageButton).Parent.Parent.Parent.Parent.Parent is Expander)
                {
                    parentExpander = ((sender as ImageButton).Parent.Parent.Parent.Parent.Parent as Expander);
                }
                // expander from object System.Diagnostics.Debug.WriteLine((sender as ImageButton).Parent.Parent.Parent.GetType());
                System.Diagnostics.Debug.WriteLine((sender as ImageButton).Parent.Parent.Parent.Parent.Parent.GetType());
                if (JType.GetJType((sender as ImageButton).Parent) == JType.Type.Property)
                {
                    ((sender as ImageButton).Parent.Parent as StackLayout).Children.Remove((View)(sender as ImageButton).Parent);
                }
                else 
                if(JType.GetJType(((sender as ImageButton).Parent.Parent.Parent as Expander).Content) == JType.Type.Object ||
                    JType.GetJType(((sender as ImageButton).Parent.Parent.Parent as Expander).Content) == JType.Type.Array)
                {
                    (((sender as ImageButton).Parent.Parent.Parent as Expander).Parent as StackLayout).Children.Remove(((sender as ImageButton).Parent.Parent.Parent as Expander));
                    //System.Diagnostics.Debug.WriteLine( JType.GetJType(((sender as ImageButton).Parent.Parent.Parent as Expander).Content));
                }
                parentExpander?.ForceUpdateSize();
                //((sender as ImageButton).Parent.Parent.Parent.Parent.Parent.Parent as Expander).ForceUpdateSize();
            }
        }

        private string CreateJson()
        {
            string res = text;
            foreach (StackLayout layout in stack_layout.Children)
            {
                try
                {
                    res = GetChildJson(layout).ToString();
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    UserDialogs.Instance.Alert(e.Message, "Error", "Ok");        
                }
            }
            return res;
        }

        private JView GetChildJson(StackLayout stackLayout) 
        {
            JView jView = new JView();
            switch (JType.GetJType(stackLayout))
            {
                case JType.Type.Property:
                    if(stackLayout.Children.Count > 2)
                    {
                        jView.Property = new JProperty((stackLayout.Children[0] as Entry).Text, (stackLayout.Children[1] as Entry).Text);
                    }else
                    {
                        jView.Property = new JProperty("", (stackLayout.Children[0] as Entry).Text);
                    }
                    
                    break;
                case JType.Type.Array:
                    jView.Array = new JArray();
                    break;
                case JType.Type.Object:
                    jView.Obj = new JObject();
                    break;
            }

            foreach (View view in stackLayout.Children)
            {
                if(view is StackLayout)
                {
                    //System.Diagnostics.Debug.WriteLine("Child is Stacklayout");
                    JView v = GetChildJson(view as StackLayout);

                    jView.Add(v.Property.Name, v.Property);
                    //jView.Add("", v.Array);
                    //jView.Add("", v.Obj);
                }
                else
                if(view is Expander)
                {
                    //System.Diagnostics.Debug.WriteLine("Child is Expander");
                    if((view as Expander).Content is StackLayout)
                    {
                        JView v = GetChildJson((view as Expander).Content as StackLayout);
                        string key = "";
                        if(((view as Expander).Header as StackLayout).Children[1] is Entry)
                        {
                            key = (((view as Expander).Header as StackLayout).Children[1] as Entry).Text;
                        }
                        if(v.Property != null)
                        {
                            jView.Add(key, v.Property);
                        }else
                        if(v.Obj != null)
                        {
                            jView.Add(key, v.Obj);
                        }else
                        if(v.Array != null)
                        {
                            jView.Add(key, v.Array);
                        }
                    }
                }
            }
            return jView;
        }

        public class JView
        {
            public JArray Array { get; set; }
            public JObject Obj { get; set; }
            public JProperty Property { get; set; }

            public void Add(string key, JProperty property)
            {
                if (Array != null)
                {
                    Array.Add(property.Value);
                } else
                if (Obj != null)
                {
                    Obj.Add(key, property.Value);
                }
                else
                if (Property != null) 
                {
                    Property = new JProperty(key, property.Value);
                }
            }
            public void Add(string key, JObject property)
            {
                if (Array != null)
                {
                    Array.Add(property);
                }
                else
                if (Obj != null)
                {
                    Obj.Add(key, property);
                }
            }

            public void Add(string key, JArray property)
            {
                if (Array != null)
                {
                    Array.Add(property);
                }
                else
                if (Obj != null)
                {
                    Obj.Add(key, property);
                }
            }

            public override string ToString()
            {
                try
                {
                    if (Array != null)
                    {
                        return Array.ToString();
                    }
                    else
                                    if (Obj != null)
                    {
                        return Obj.ToString();
                    }
                    else
                                    if (Property != null)
                    {
                        return Property.ToString();
                    }
                    else
                    {
                        return "";
                    }
                }catch(Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public class ExpanderIconRotationConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (bool)value ? 180 : 90;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class JType
        {
            public static readonly BindableProperty JTypeProperty = BindableProperty.CreateAttached("JType", typeof(Type), typeof(JType), defaultValue: Type.None);

            public static Type GetJType(BindableObject view)
            {
                return (Type)view.GetValue(JTypeProperty);
            }

            public static void SetJtype(BindableObject view, Type value)
            {
                view.SetValue(JTypeProperty, value);
            }

            public enum Type 
            {
                None,
                Property,
                Array,
                Object
            }
        }
    }
}