using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Csml {
    public sealed class UnorderedList : ContainerWithSpecificChildren<UnorderedList> {
        public UnorderedList() : base("ul", "li", "List", "Unordered") {

        }
    }
    public sealed class OrderedList : ContainerWithSpecificChildren<OrderedList> {
        public OrderedList() : base("ol", "li", "List", "Ordered") {

        }
    }



}