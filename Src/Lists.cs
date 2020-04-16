using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Csml {
    public sealed class UnorderedList : ContainerWithSpecificChildren<UnorderedList> {
        public UnorderedList() : base("ul", "li") {

        }
    }
    public sealed class OrderedList : ContainerWithSpecificChildren<OrderedList> {
        public OrderedList() : base("ol", "li") {

        }
    }



}