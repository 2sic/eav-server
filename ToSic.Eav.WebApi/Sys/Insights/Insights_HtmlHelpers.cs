using System;
using System.Linq;
using System.Text;
using System.Web;
using ToSic.Razor.Blade;
using ToSic.Razor.Markup;

namespace ToSic.Eav.WebApi.Sys
{
    internal class SpecialField
    {

        public SpecialField(object value, string styles)
        {
            Styles = styles;
            Value = value;
        }
        public object Value { get; }
        public string Styles { get; }

        public static SpecialField Right(object value) => new SpecialField(value, "text-align: right; padding - right: 5px;");
    }

    public partial class InsightsControllerReal
    {
        protected static ITag HeadFields(params object[] fields)
            => Tag.Thead(Tag.Tr(
                fields.Select(fresh => Tag.Th(HtmlEncode((fresh ?? "").ToString()))).ToArray<object>()
            ));

        protected static ITag RowFields(params object[] fields)
            => Tag.Tr(
                fields
                    .Select(fresh =>
                    {
                        var data = fresh;
                        string styles = null;
                        if (fresh is SpecialField special)
                        {
                            data = special.Value;
                            styles = special.Styles;
                        }
                        
                        var td = Tag.Td((data ?? "").ToString());
                        if (styles != null)
                            td.Style(styles);
                        return td;
                    })
                .ToArray<object>());

        protected const string JsTableSortCdn = "https://cdnjs.cloudflare.com/ajax/libs/tablesort/5.2.1/";

        protected static TagBase JsTableSort(string id = "table")
            => Tag.Script().Src(JsTableSortCdn + "tablesort.min.js")
               + Tag.Script().Src(JsTableSortCdn + "sorts/tablesort.number.min.js")
               + Tag.Script($"new Tablesort(document.getElementById('{id}'));");




        protected static string PageStyles() =>
            @"
<style>
.logIds {
    color: darkgray;
}

.codePeek {
    color: blue;
}

.result {
    color: green;
}
/* first ol level needs more padding, because it can number up to 4 digits */
body>ol {
    padding-inline-start: 40px;
}

/* all other OL levels need smaller padding, as they should be aligned nicely */
ol {
    padding-inline-start: 23px;
    list-style: none; 
    counter-reset: li;
}
ol li::before {
    counter-increment: li;
    content: '.'counter(li); 
    display: inline-block; 
    width: 1em; margin-left: -1.5em;
    margin-right: 0.5em; 
    text-align: right; 
    direction: rtl;
    font-weight: bold;
}

/* Hover guides - don't work, as the entire area is hovered */
/* li:hover {
  background-color: #ddd;
}
li:hover li {
  background-color: #eee;
}
*/

/* Put border around the first  */

/* Color each level to better see which go together */
ol li::before                   { color: black;         }
ol ol li::before                { color: blue;          }
ol ol ol li::before             { color: brown;         }
ol ol ol ol li::before          { color: red;           }
ol ol ol ol ol li::before       { color: darkorange;    }
ol ol ol ol ol ol li::before    { color: firebrick;     }
ol ol ol ol ol ol ol li::before { color: turquoise;     }
ol ol ol ol ol ol ol ol li::before                   { color: black;        }
ol ol ol ol ol ol ol ol ol li::before                { color: blue;         }
ol ol ol ol ol ol ol ol ol ol li::before             { color: brown;        }
ol ol ol ol ol ol ol ol ol ol ol li::before          { color: red;          }
ol ol ol ol ol ol ol ol ol ol ol ol li::before       { color: darkorange;   }
ol ol ol ol ol ol ol ol ol ol ol ol ol li::before    { color: firebrick;    }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol li::before { color: turquoise;    }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li::before                   { color: black;       }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li::before                { color: blue;        }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li::before             { color: brown;       }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li::before          { color: red;         }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li::before       { color: darkorange;  }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li::before    { color: firebrick;   }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li::before { color: turquoise;   }


/* Guide Lines */
ol li {
  border-left-style: solid;
  border-left-width: 1px;
}

ol li                   { border-left-color: black;         }
ol ol li                { border-left-color: blue;          }
ol ol ol li             { border-left-color: brown;         }
ol ol ol ol li          { border-left-color: red;           }
ol ol ol ol ol li       { border-left-color: darkorange;    }
ol ol ol ol ol ol li    { border-left-color: firebrick;     }
ol ol ol ol ol ol ol li { border-left-color: turquoise;     }
ol ol ol ol ol ol ol ol li                   { border-left-color: black;         }
ol ol ol ol ol ol ol ol ol li                { border-left-color: blue;          }
ol ol ol ol ol ol ol ol ol ol li             { border-left-color: brown;         }
ol ol ol ol ol ol ol ol ol ol ol li          { border-left-color: red;           }
ol ol ol ol ol ol ol ol ol ol ol ol li       { border-left-color: darkorange;    }
ol ol ol ol ol ol ol ol ol ol ol ol ol li    { border-left-color: firebrick;     }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol li { border-left-color: turquoise;     }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li                   { border-left-color: black;         }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li                { border-left-color: blue;          }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li             { border-left-color: brown;         }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li          { border-left-color: red;           }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li       { border-left-color: darkorange;    }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li    { border-left-color: firebrick;     }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li { border-left-color: turquoise;     }


/***** Time Styling *****/

span.time {
  float: right;
}
span.time span.emoji {
  font-size: smaller;
}

ol li                       span.time { color: black;         }
ol ol li                    span.time { color: blue;          }
ol ol ol li                 span.time { color: brown;         }
ol ol ol ol li              span.time { color: red;           }
ol ol ol ol ol li           span.time { color: darkorange;    }
ol ol ol ol ol ol li        span.time { color: firebrick;     }
ol ol ol ol ol ol ol li     span.time { color: turquoise;     }
ol ol ol ol ol ol ol ol li                       span.time { color: black;         }
ol ol ol ol ol ol ol ol ol li                    span.time { color: blue;          }
ol ol ol ol ol ol ol ol ol ol li                 span.time { color: brown;         }
ol ol ol ol ol ol ol ol ol ol ol li              span.time { color: red;           }
ol ol ol ol ol ol ol ol ol ol ol ol li           span.time { color: darkorange;    }
ol ol ol ol ol ol ol ol ol ol ol ol ol li        span.time { color: firebrick;     }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol li     span.time { color: turquoise;     }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li                       span.time { color: black;         }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li                    span.time { color: blue;          }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li                 span.time { color: brown;         }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li              span.time { color: red;           }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li           span.time { color: darkorange;    }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li        span.time { color: firebrick;     }
ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol ol li     span.time { color: turquoise;     }


span.log-line:hover {
  background-color: #ddd;
}
</style>";

        protected static string HoverLabel(string label, string text, string classes)
            => Tag.Span(label).Class(classes).Title(text).ToString();

        protected static string HtmlEncode(string text)
        {
            if (text == null) return "";
            var chars = HttpUtility.HtmlEncode(text).ToCharArray();
            var result = new StringBuilder(text.Length + (int)(text.Length * 0.1));

            foreach (var c in chars)
            {
                var value = Convert.ToInt32(c);
                if (value > 127)
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        protected static string EmojiTrueFalse(bool value) => HtmlEncode(value ? "✅" : "⛔");

    }
}
