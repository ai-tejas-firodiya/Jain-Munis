import React from 'react'
import { cn } from '@/lib/utils'

interface CardProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode
}

interface CardHeaderProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode
}

interface CardContentProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode
}

interface CardFooterProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode
}

const Card = React.forwardRef<HTMLDivElement, CardProps>(
  ({ className, children, ...props }, ref) => (
    <div
      ref={ref}
      className={cn(
        "bg-white rounded-lg border border-gray-200 shadow-sm",
        className
      )}
      {...props}
    >
      {children}
    </div>
  )
)

const CardHeader = React.forwardRef<HTMLDivElement, CardHeaderProps>(
  ({ className, children, ...props }, ref) => (
    <div
      ref={ref}
      className={cn("p-6 border-b border-gray-200", className)}
      {...props}
    >
      {children}
    </div>
  )
)

const CardContent = React.forwardRef<HTMLDivElement, CardContentProps>(
  ({ className, children, ...props }, ref) => (
    <div
      ref={ref}
      className={cn("p-6", className)}
      {...props}
    >
      {children}
    </div>
  )
)

const CardFooter = React.forwardRef<HTMLDivElement, CardFooterProps>(
  ({ className, children, ...props }, ref) => (
    <div
      ref={ref}
      className={cn("p-6 border-t border-gray-200", className)}
      {...props}
    >
      {children}
    </div>
  )
)

const CardTitle = React.forwardRef<HTMLParagraphElement, React.HTMLAttributes<HTMLHeadingElement>>(
  ({ className, children, ...props }, ref) => (
    <h3
      ref={ref}
      className={cn("text-lg font-semibold leading-none tracking-tight", className)}
      {...props}
    >
      {children}
    </h3>
  )
)

const CardDescription = React.forwardRef<HTMLParagraphElement, React.HTMLAttributes<HTMLParagraphElement>>(
  ({ className, children, ...props }, ref) => (
    <p
      ref={ref}
      className={cn("text-sm text-gray-600", className)}
      {...props}
    >
      {children}
    </p>
  )
)

Card.displayName = "Card"
CardHeader.displayName = "CardHeader"
CardContent.displayName = "CardContent"
CardFooter.displayName = "CardFooter"
CardTitle.displayName = "CardTitle"
CardDescription.displayName = "CardDescription"

export { Card, CardHeader, CardContent, CardFooter, CardTitle, CardDescription }