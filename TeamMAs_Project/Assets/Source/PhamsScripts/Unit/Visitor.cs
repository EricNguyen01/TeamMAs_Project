using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class Visitor : MonoBehaviour
    {
        public VisitorSO visitorSO { get; private set; }
        public VisitorPool poolContainsThisVisitor { get; private set; }

        public void SetPoolContainsThisVisitor(VisitorPool visitorPool)
        {
            poolContainsThisVisitor = visitorPool;
        }
    }
}
