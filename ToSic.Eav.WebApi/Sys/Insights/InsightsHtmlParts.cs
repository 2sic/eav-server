using ToSic.Razor.Blade;

namespace ToSic.Eav.WebApi.Sys
{
    internal class InsightsHtmlParts
    {
        #region Table / Sort

        internal const string JsTableSortCdn = "https://cdnjs.cloudflare.com/ajax/libs/tablesort/5.2.1/";

        internal static IHtmlTag JsTableSort(string id = "table")
            => Tag.Script().Src(JsTableSortCdn + "tablesort.min.js")
               + Tag.Script().Src(JsTableSortCdn + "sorts/tablesort.number.min.js")
               + Tag.Script($"new Tablesort(document.getElementById('{id}'));") as IHtmlTag;

        #endregion

        internal static string PageStyles() =>
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
  font-size: small;
}
/*
span.time span.emoji {
  font-size: smaller;
}
*/

span.time-of-total {
  color: black;
  font-weight: bold;
    width: 33px;
/*    font-size: small; */
    display: inline-block;
    text-align: right;
}

ol li                       span.time { color: black;         }
ol ol li                    span.time { color: #222;          }
ol ol ol li                 span.time { color: #444;         }
ol ol ol ol li              span.time { color: #666;           }
ol ol ol ol ol li           span.time { color: #888;    }
ol ol ol ol ol ol li        span.time { color: #aaa;     }

/*
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
*/

span.log-line:hover {
  background-color: #ddd;
}

</style>";
    }
}
